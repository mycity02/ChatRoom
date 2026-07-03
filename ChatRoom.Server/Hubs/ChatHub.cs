using ChatRoom.Server.Data;
using ChatRoom.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace ChatRoom.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _dbContext;
        private static readonly ConcurrentDictionary<int, string> UserConnection = new();

        public ChatHub(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 注册用户连接，将用户ID与连接ID关联，并加载该用户的会话列表。
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RegisterUser(int userId)
        {
            // 将用户ID与当前连接ID关联
            UserConnection[userId] = Context.ConnectionId;
            // 加载该用户的会话列表
            await LoadConversation(userId);
        }

        /// <summary>
        /// 加载指定用户的会话列表，包括与其他用户的私聊会话及最后一条消息。
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task LoadConversation(int userId)
        {
            var conversationList = await _dbContext.PrivateConversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Select(c => new
                {
                    // 获取会话ID、对方用户ID、对方用户名、最后一条消息及其发送时间
                    c.ConversationId,
                    OtherUserId = c.User1Id == userId ? c.User2Id : c.User1Id,
                    OtherUserName = c.User1Id == userId
                        ? (c.User2 != null ? c.User2.UserName : string.Empty)
                        : (c.User1 != null ? c.User1.UserName : string.Empty),
                    LastMessage = c.ChatMessages
                        .OrderByDescending(m => m.SendTime)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? string.Empty,
                    LastMessageTime = c.ChatMessages
                        .OrderByDescending(m => m.SendTime)
                        .Select(m => m.SendTime)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

           
            if (UserConnection.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("LoadConversations", conversationList);
            }
        }

        /// <summary>
        /// 发送私聊消息，首先获取或创建私聊会话，然后保存消息并通知发送者和接收者。
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <param name="senderName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendPrivateMessage(int senderId, int receiverId, string senderName, string message)
        {
            var conversation = await GetOrCreatePrivateConversationAsync(senderId, receiverId);

            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceivedId = receiverId,
                UserName = senderName,
                Content = message,
                ConversationId = conversation.ConversationId,
                SendTime = DateTime.Now
            };

            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

            // 1. 发给自己，让自己的窗口显示刚发出的消息
            await Clients.Caller.SendAsync("ReceivePrivateMessage", chatMessage);

            // 2.如果接收者在线，则发送消息给接收者
            if (UserConnection.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", chatMessage);
                await LoadConversation(receiverId);
            }

            await LoadConversation(senderId);
        }

        /// <summary>
        /// 获取或创建私聊会话，如果指定的两个用户之间已经存在会话，则返回该会话；否则创建一个新的会话并返回。
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        private async Task<PrivateConversation> GetOrCreatePrivateConversationAsync(int senderId, int receiverId)
        {
            var conversation = await _dbContext.PrivateConversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == senderId && c.User2Id == receiverId) ||
                    (c.User1Id == receiverId && c.User2Id == senderId));

            if (conversation != null)
                return conversation;

            conversation = new PrivateConversation
            {
                User1Id = senderId,
                User2Id = receiverId,
                CreateTime = DateTime.Now
            };

            _dbContext.PrivateConversations.Add(conversation);
            await _dbContext.SaveChangesAsync();
            return conversation;
        }

        /// <summary>
        /// 发送群聊消息，将消息保存到数据库并广播给所有连接的客户端。
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(int userId, string userName, string message)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = userId,
                UserName = userName,
                Content = message,
                SendTime = DateTime.Now
            };

            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();
            await Clients.All.SendAsync("ReceiveMessage", userName, message);
        }

        /// <summary>
        /// 在客户端连接到Hub时，加载最近的50条群聊消息并发送给调用者。
        /// </summary>
        /// <returns></returns>
        //public override async Task OnConnectedAsync()
        //{
        //    var recentMessages = await _dbContext.ChatMessages
        //        .Where(m => m.ReceivedId == null)
        //        .OrderByDescending(m => m.SendTime)
        //        .Take(50)
        //        .OrderBy(m => m.SendTime)
        //        .ToListAsync();

        //    await Clients.Caller.SendAsync("LoadHistory", recentMessages);
        //    await base.OnConnectedAsync();
        //}

        /// <summary>
        /// 在客户端断开连接时，从UserConnection字典中移除该连接ID对应的用户ID。
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var pair in UserConnection.Where(pair => pair.Value == Context.ConnectionId).ToList())
            {
                UserConnection.TryRemove(pair.Key, out _);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}