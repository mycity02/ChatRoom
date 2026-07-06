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

        /// <summary>
        /// 直接添加好友。
        /// 这是旧逻辑：添加好友后会直接创建私聊会话。
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        public async Task<ConversationDto?> AddFriendAsync(FriendDto friendDto)
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

                return new ConversationDto
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

            return new ConversationDto
            {
                ConversationId = privateConversation.ConversationId,
                OtherUserId = friendUser.Id,
                OtherUserName = friendUser.UserName,
                LastMessage = lastMessage?.Content ?? string.Empty,
                LastMessageTime = lastMessage?.SendTime ?? privateConversation.CreateTime
            };
        }

        /// <summary>
        /// 发送好友申请。
        /// 这里只创建 FriendShip 申请记录，不创建私聊会话。
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        public async Task<FriendRequestDto?> AddFriendRequestAsync(FriendDto friendDto)
        {
            // 当前用户是发送好友申请的人。
            var currentUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Id == friendDto.CurrentUserId);

            if (currentUser == null)
                return null;

            // 根据用户名查找被申请的用户。
            var friendUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == friendDto.FriendName);

            if (friendUser == null)
                return null;

            // 不能添加自己。
            if (friendUser.Id == friendDto.CurrentUserId)
                return null;

            // 只要两个人之间已经有申请或好友关系，就不再重复创建。
            var friendshipExist = await _appDbContext.Friendships
                .AnyAsync(f =>
                    (f.RequestId == friendDto.CurrentUserId && f.ReceivedId == friendUser.Id) ||
                    (f.RequestId == friendUser.Id && f.ReceivedId == friendDto.CurrentUserId));

            if (friendshipExist)
                return null;

            var newFriendship = new FriendShip
            {
                RequestId = friendDto.CurrentUserId,
                ReceivedId = friendUser.Id,
                Status = "pending",
                CreateTime = DateTime.Now
            };

            await _appDbContext.Friendships.AddAsync(newFriendship);
            await _appDbContext.SaveChangesAsync();

            return new FriendRequestDto
            {
                // FriendShip 表里的申请记录 Id
                FriendshipId = newFriendship.Id,

                // 申请人，也就是当前登录用户
                RequestUserId = currentUser.Id,
                RequestUserName = currentUser.UserName,

                // 被申请人，也就是输入的好友用户名对应的用户
                ReceivedUserId = friendUser.Id,
                ReceivedUserName = friendUser.UserName,

                // 新申请默认是 pending
                Status = newFriendship.Status,

                // 申请创建时间
                CreateTime = newFriendship.CreateTime
            };
        }

        /// <summary>
        /// 获取当前用户收到的好友申请。
        /// 这里只加载 pending 状态，因为收到列表需要展示“同意 / 拒绝”按钮。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>别人发给当前用户的待处理申请。</returns>
        public async Task<List<FriendRequestDto>> GetReceivedFriendRequestsAsync(int userId)
        {
            return await _appDbContext.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .Where(f => f.ReceivedId == userId && f.Status == "pending")
                .OrderByDescending(f => f.CreateTime)
                .Select(f => new FriendRequestDto
                {
                    FriendshipId = f.Id,
                    RequestUserId = f.RequestId,
                    RequestUserName = f.Requester.UserName,
                    ReceivedUserId = f.ReceivedId,
                    ReceivedUserName = f.Receiver.UserName,
                    Status = f.Status,
                    CreateTime = f.CreateTime
                })
                .ToListAsync();
        }

        /// <summary>
        /// 获取当前用户发出的好友申请。
        /// 发出列表保留所有状态，方便用户看到 pending / accepted / rejected。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>当前用户发出的申请记录。</returns>
        public async Task<List<FriendRequestDto>> GetSentFriendRequestsAsync(int userId)
        {
            return await _appDbContext.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .Where(f => f.RequestId == userId)
                .OrderByDescending(f => f.CreateTime)
                .Select(f => new FriendRequestDto
                {
                    FriendshipId = f.Id,
                    RequestUserId = f.RequestId,
                    RequestUserName = f.Requester.UserName,
                    ReceivedUserId = f.ReceivedId,
                    ReceivedUserName = f.Receiver.UserName,
                    Status = f.Status,
                    CreateTime = f.CreateTime
                })
                .ToListAsync();
        }

        /// <summary>
        /// 同意好友申请。
        /// 这里才会把申请状态改成 accepted，并创建私聊会话。
        /// </summary>
        /// <param name="friendshipId">FriendShip 表里的申请记录 Id，不是用户 Id。</param>
        /// <returns></returns>
        public async Task<AcceptFriendRequestResultDto?> AcceptAddFriendRequestAsync(int friendshipId)
        {
            var friendship = await _appDbContext.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            // 申请不存在，或者已经处理过，就不能再次同意。
            if (friendship == null || friendship.Status != "pending")
                return null;

            friendship.Status = "accepted";

            // 正常申请流程里，同意前没有私聊会话。
            // 这里仍然查一次，是为了防止旧数据或重复点击造成重复会话。
            var privateConversation = await _appDbContext.PrivateConversations
                .FirstOrDefaultAsync(session =>
                    (session.User1Id == friendship.RequestId && session.User2Id == friendship.ReceivedId) ||
                    (session.User1Id == friendship.ReceivedId && session.User2Id == friendship.RequestId));

            if (privateConversation == null)
            {
                privateConversation = new PrivateConversation
                {
                    User1Id = friendship.RequestId,
                    User2Id = friendship.ReceivedId,
                    CreateTime = DateTime.Now
                };

                _appDbContext.PrivateConversations.Add(privateConversation);
            }

            // 先保存会话，确保 ConversationId 已经生成。
            await _appDbContext.SaveChangesAsync();

            var greetingContent = "我们已经成为好友了！开始聊天吧！";

            // 同意方主动打招呼：ReceivedId 是点击同意的人，RequestId 是发起申请的人。
            var greetingMessage = new ChatMessage
            {
                SenderId = friendship.ReceivedId,
                ReceivedId = friendship.RequestId,
                UserName = friendship.Receiver.UserName,
                Content = greetingContent,
                ConversationId = privateConversation.ConversationId,
                SendTime = DateTime.Now
            };

            _appDbContext.ChatMessages.Add(greetingMessage);
            await _appDbContext.SaveChangesAsync();

            // 这个 ConversationDto 是返回给点击“同意”的人看的。
            // 点击同意的人是 ReceivedId，所以 OtherUser 是发起申请的 RequestId。
            var conversation = new ConversationDto
            {
                ConversationId = privateConversation.ConversationId,
                OtherUserId = friendship.RequestId,
                OtherUserName = friendship.Requester.UserName,
                LastMessage = greetingContent,
                LastMessageTime = greetingMessage.SendTime
            };

            var friendRequest = new FriendRequestDto
            {
                FriendshipId = friendship.Id,
                RequestUserId = friendship.RequestId,
                RequestUserName = friendship.Requester.UserName,
                ReceivedUserId = friendship.ReceivedId,
                ReceivedUserName = friendship.Receiver.UserName,
                Status = friendship.Status,
                CreateTime = friendship.CreateTime
            };

            return new AcceptFriendRequestResultDto
            {
                Conversation = conversation,
                GreetingMessage = greetingMessage,
                FriendRequest = friendRequest
            };
        }
        /// <summary>
        /// 拒绝好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        public async Task<FriendRequestDto?> RejectAddFriendRequestAsync(int friendshipId)
        {
            var friendship = await _appDbContext.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null || friendship.Status != "pending")
                return null;

            friendship.Status = "rejected";

            await _appDbContext.SaveChangesAsync();

            return new FriendRequestDto
            {
                FriendshipId = friendship.Id,

                RequestUserId = friendship.RequestId,
                RequestUserName = friendship.Requester.UserName,

                ReceivedUserId = friendship.ReceivedId,
                ReceivedUserName = friendship.Receiver.UserName,

                Status = friendship.Status,
                CreateTime = friendship.CreateTime
            };
        }
    }
}




