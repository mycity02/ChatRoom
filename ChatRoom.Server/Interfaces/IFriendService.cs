using ChatRoom.Server.Dto;

namespace ChatRoom.Server.Interfaces
{
    public interface IFriendService
    {
        /// <summary>
        /// 直接添加好友
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        Task<ConversationDto?> AddFriendAsync(FriendDto friendDto);

        /// <summary>
        /// 发送好友申请
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        Task<FriendRequestDto?> AddFriendRequestAsync(FriendDto friendDto);

        /// <summary>
        /// 同意好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        Task<AcceptFriendRequestResultDto?> AcceptAddFriendRequestAsync(int friendshipId);

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
        Task<FriendRequestDto?> RejectAddFriendRequestAsync(int friendshipId);

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<FriendItemDto>> GetFriendItemListAsync(int userId);
    }
}





