using ChatRoom.Server.Data;
using ChatRoom.Server.Dto;
using ChatRoom.Server.Interfaces;
using ChatRoom.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserConnectionManager _userConnectionManager;

        public ChatHub(AppDbContext dbContext, 
            IUserConnectionManager userConnectionManager)
        {
            _dbContext = dbContext;
            _userConnectionManager = userConnectionManager;
        }

        /// <summary>
        /// 注册用户连接，将用户ID与连接ID关联，并加载该用户的会话列表。
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RegisterUser(int userId)
        {
            // 保存当前用户的SignalR的连接
            _userConnectionManager.AddConnection(userId, Context.ConnectionId);
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
                .Select(c => new ConversationDto
                {
                    // 获取会话ID、对方用户ID、对方用户名、最后一条消息及其发送时间
                    ConversationId = c.ConversationId,
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

           
            if (_userConnectionManager.TryGetConnection(userId, out var connectionId))
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

            var messageDto = new
            {
                SenderId = chatMessage.SenderId,
                ReceivedId = chatMessage.ReceivedId,
                UserName = chatMessage.UserName,
                Content = chatMessage.Content,
                ConversationId = chatMessage.ConversationId,
                SendTime = chatMessage.SendTime
            };

            // 1. 发给自己，让自己的窗口显示刚发出的消息
            await Clients.Caller.SendAsync("ReceivePrivateMessage", messageDto);

            // 2.如果接收者在线，则发送消息给接收者
            if (_userConnectionManager.TryGetConnection(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", messageDto);
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
        
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // 用户断开时移除连接记录
            _userConnectionManager.RemoveConnection(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendGroupMessage(long groupId, int senderId, 
            string userName, string message)
        {
            if (groupId <= 0 || senderId <= 0 || string.IsNullOrWhiteSpace(message))
                return;

            // 只允许群内成员发送信息
            var isMember = await _dbContext.GroupMembers
                .AnyAsync(member =>
                    member.GroupId == groupId &&
                    member.UserId == senderId);

            if (!isMember)
                throw new HubException("你不是该群成员");

            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                UserName = userName,
                Content = message.Trim(),
                GroupId = groupId,
                SendTime = DateTime.Now
            };

            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

            // 查出群成员 只推送给在线用户
            var memberIds = await _dbContext.GroupMembers
                .Where(member => member.GroupId == groupId)
                .Select(member => member.UserId)
                .ToListAsync();

            var connectionIds = memberIds
                .Select(memberId =>
                    _userConnectionManager.TryGetConnection(
                        memberId, out var connectionId) ? connectionId : null)
                .Where(connectionId => connectionId != null)
                .Cast<string>()
                .Distinct()
                .ToList();

            if (connectionIds.Count == 0)
                return;

            var messageDto = new
            {
                SenderId = chatMessage.SenderId,
                UserName = chatMessage.UserName,
                Content = chatMessage.Content,
                GroupId = chatMessage.GroupId,
                SendTime = chatMessage.SendTime
            };

            // 向在线用户推送消息
            await Clients.Clients(connectionIds)
                .SendAsync("ReceiveGroupMessage", groupId, messageDto);
        }
    }
}


