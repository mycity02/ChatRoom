using ChatRoom.Server.Dto;

namespace ChatRoom.Server.Interfaces
{
    public interface IGroupService
    {
        /// <summary>
        /// 创建群聊 并将群主和选中的成员加入该群
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<GroupDto?> CreateGroupAsync(CreateGroupDto group);

        /// <summary>
        /// 获取当前用户加入的群聊
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<GroupDto>> GetMyGroupsAsync(int userId);

        /// <summary>
        /// 获取当前用户有权限查看的最近群聊消息。
        /// </summary>
        Task<List<GroupMessageDto>?> GetGroupMessagesAsync(long groupId, int userId);
    }
}
