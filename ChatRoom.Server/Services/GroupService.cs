using ChatRoom.Server.Data;
using ChatRoom.Server.Dto;
using ChatRoom.Server.Interfaces;
using ChatRoom.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Services
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _appDbContext;

        public GroupService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<GroupDto?> CreateGroupAsync(CreateGroupDto group)
        {
            if (group.OwnerId <= 0 || group.GroupName == null)
                return null;

            // 获取成员id列表
            var memberIds = group.MemberIds
                .Where(memberId => memberId != group.OwnerId && memberId > 0)
                .Distinct()
                .ToList();

            // 群聊成员数量不能少于1
            if (memberIds.Count == 0)
                return null;

            // 判断群主是否存在
            var ownerExist = await _appDbContext.Users
                .AnyAsync(user => user.Id == group.OwnerId);

            // 群主不存在终止创建
            if (!ownerExist) return null;

            // 群成员是否存在
            var membersExist = await _appDbContext.Users
                .CountAsync(user => memberIds.Contains(user.Id));

            // 群成员不存在终止创建
            if (membersExist != memberIds.Count)
                return null;

            // 只能邀请自己的已添加好友进入群聊。
            var friendIds = await _appDbContext.Friendships
                .Where(friendship =>
                    friendship.Status == "accepted" &&
                    (friendship.RequestId == group.OwnerId ||
                     friendship.ReceivedId == group.OwnerId))
                .Select(friendship =>
                    friendship.RequestId == group.OwnerId
                        ? friendship.ReceivedId
                        : friendship.RequestId)
                .ToListAsync();

            // 成员不是好友
            if (memberIds.Any(id => !friendIds.Contains(id)))
                return null;

            // 创建群聊
            var newGroup = new Group
            {
                OwnerId = group.OwnerId,
                GroupName = group.GroupName.Trim(),
                CreateTime = DateTime.Now
            };

            // EF Core 会在保存 Group 时一并保存 Members。
            newGroup.Members.Add(new GroupMember
            {
                UserId = group.OwnerId,
                JoinTime = DateTime.Now
            });

            foreach (var memberId in memberIds)
            {
                newGroup.Members.Add(new GroupMember
                {
                    UserId = memberId,
                    JoinTime = DateTime.Now
                });
            }

            _appDbContext.Groups.Add(newGroup);
            await _appDbContext.SaveChangesAsync();

            return new GroupDto
            {
                GroupId = newGroup.GroupId,
                GroupName = newGroup.GroupName,
                OwnerId = newGroup.OwnerId,
                LastMessage = string.Empty,
                CreateTime = newGroup.CreateTime
            };
        }

        /// <summary>
        /// 获取我加入的群聊
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<GroupDto>> GetMyGroupsAsync(int userId)
        {
            return await _appDbContext.GroupMembers
                .Where(member => member.UserId == userId)
                .OrderByDescending(member => member.Group!.CreateTime)
                .Select(member => new GroupDto
                {
                    GroupId = member.GroupId,
                    GroupName = member.Group!.GroupName,
                    OwnerId = member.Group.OwnerId,
                    LastMessage = string.Empty,
                    CreateTime = member.Group.CreateTime
                })
                .ToListAsync();
        }
        /// <summary>
        /// 仅允许群成员读取该群历史消息，避免通过 groupId 查看其他群内容。
        /// </summary>
        public async Task<List<GroupMessageDto>?> GetGroupMessagesAsync(long groupId, int userId)
        {
            var isMember = await _appDbContext.GroupMembers
                .AnyAsync(member => member.GroupId == groupId && member.UserId == userId);

            if (!isMember)
                return null;

            // 先取最新 50 条，再按时间正序返回，客户端可直接追加到消息列表。
            return await _appDbContext.ChatMessages
                .AsNoTracking()
                .Where(message => message.GroupId == groupId)
                .OrderByDescending(message => message.SendTime)
                .Take(50)
                .OrderBy(message => message.SendTime)
                .Select(message => new GroupMessageDto
                {
                    SenderId = message.SenderId,
                    UserName = message.UserName,
                    Content = message.Content,
                    SendTime = message.SendTime
                })
                .ToListAsync();
        }
    }
}
