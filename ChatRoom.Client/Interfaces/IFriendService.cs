using ChatRoom.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
        Task<Conversation?> AddFriendAsync(int currentUserId, string friendName);
    }
}
