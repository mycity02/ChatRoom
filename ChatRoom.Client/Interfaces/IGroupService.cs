using ChatRoom.Client.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Interfaces
{
    public interface IGroupService
    {
        /// <summary>
        /// 创建群聊
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="groupName"></param>
        /// <param name="membersId"></param>
        /// <returns></returns>
        Task<GroupDto?> CreateGroupAsync(int ownerId, string groupName, List<int> membersId);
        
        /// <summary>
        /// 获取我加入的群聊
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<GroupDto>> GetMyGroupAsync(int userId);
    }
}
