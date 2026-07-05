using ChatRoom.Server.Dto;
using ChatRoom.Server.Models;

namespace ChatRoom.Server.Interfaces
{
    public interface IFriendService
    {
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        Task<Conversation?> AddFriendAsync(FriendDto friendDto);
    }
}
