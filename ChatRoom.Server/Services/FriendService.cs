using ChatRoom.Server.Data;
using ChatRoom.Server.Dto;
using ChatRoom.Server.Interfaces;
using ChatRoom.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Services
{
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _appDbContext;

        public FriendService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Conversation?> AddFriendAsync(FriendDto friendDto)
        {
            var currentUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Id == friendDto.CurrentUserId);

            if (currentUser == null)
                return null;

            var friendUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == friendDto.FriendName);

            if (friendUser == null)
                return null;

            if (friendUser.Id == friendDto.CurrentUserId)
                return null;

            var friendshipExists = await _appDbContext.Friendships.AnyAsync(f =>
                (f.RequestId == friendDto.CurrentUserId && f.ReceivedId == friendUser.Id) ||
                (f.RequestId == friendUser.Id && f.ReceivedId == friendDto.CurrentUserId));

            var shouldCreateGreeting = !friendshipExists;

            if (!friendshipExists)
            {
                var newFriendship = new FriendShip
                {
                    RequestId = friendDto.CurrentUserId,
                    ReceivedId = friendUser.Id,
                    Status = "accepted",
                    CreateTime = DateTime.Now,
                };

                _appDbContext.Friendships.Add(newFriendship);
            }

            var privateConversation = await _appDbContext.PrivateConversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == friendDto.CurrentUserId && c.User2Id == friendUser.Id) ||
                    (c.User1Id == friendUser.Id && c.User2Id == friendDto.CurrentUserId));

            if (privateConversation == null)
            {
                privateConversation = new PrivateConversation
                {
                    User1Id = friendDto.CurrentUserId,
                    User2Id = friendUser.Id,
                    CreateTime = DateTime.Now
                };

                _appDbContext.PrivateConversations.Add(privateConversation);
            }

            await _appDbContext.SaveChangesAsync();

            if (shouldCreateGreeting)
            {
                var greetingContent = "我们已经成为好友了！开始聊天吧！";

                var greetingMessage = new ChatMessage
                {
                    SenderId = friendDto.CurrentUserId,
                    ReceivedId = friendUser.Id,
                    UserName = currentUser.UserName,
                    Content = greetingContent,
                    ConversationId = privateConversation.ConversationId,
                    SendTime = DateTime.Now
                };

                _appDbContext.ChatMessages.Add(greetingMessage);
                await _appDbContext.SaveChangesAsync();

                return new Conversation
                {
                    ConversationId = privateConversation.ConversationId,
                    OtherUserId = friendUser.Id,
                    OtherUserName = friendUser.UserName,
                    LastMessage = greetingContent,
                    LastMessageTime = greetingMessage.SendTime
                };
            }

            var lastMessage = await _appDbContext.ChatMessages
                .Where(m => m.ConversationId == privateConversation.ConversationId)
                .OrderByDescending(m => m.SendTime)
                .Select(m => new { m.Content, m.SendTime })
                .FirstOrDefaultAsync();

            return new Conversation
            {
                ConversationId = privateConversation.ConversationId,
                OtherUserId = friendUser.Id,
                OtherUserName = friendUser.UserName,
                LastMessage = lastMessage?.Content ?? string.Empty,
                LastMessageTime = lastMessage?.SendTime ?? privateConversation.CreateTime
            };
        }
    }
}
