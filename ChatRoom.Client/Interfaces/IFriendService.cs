using ChatRoom.Client.Dto;
using ChatRoom.Client.Models;

namespace ChatRoom.Client.Interfaces
{
    public interface IFriendService
    {
        /// <summary>
        /// 添加好友并返回与该好友的会话信息
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="friendName"></param>
        /// <returns></returns>
        Task<ConversationDto?> AddFriendAsync(int currentUserId, string friendName);

        /// <summary>
        /// 好友申请
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="friendName"></param>
        /// <returns></returns>
        Task<FriendRequestDto?> AddFriendRequestAsync(int currentUserId, string friendName);

        /// <summary>
        /// 同意好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        Task<ConversationDto?> AcceptFriendRequestAsync(int friendshipId);

        /// <summary>
        /// 获取当前用户收到的好友申请。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>别人发给当前用户的好友申请列表。</returns>
        Task<List<FriendRequestDto>> GetReceivedFriendRequestsAsync(int userId);

        /// <summary>
        /// 获取当前用户发出的好友申请。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>当前用户发出的好友申请列表。</returns>
        Task<List<FriendRequestDto>> GetSentFriendRequestsAsync(int userId);

        /// <summary>
        /// 拒绝好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        Task<FriendRequestDto?> RejectFriendRequestAsync(int friendshipId);

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<FriendItem>> GetFriendListAsync(int userId);
    }
}

