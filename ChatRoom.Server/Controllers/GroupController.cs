using ChatRoom.Server.Dto;
using ChatRoom.Server.Hubs;
using ChatRoom.Server.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatRoom.Server.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUserConnectionManager _userConnectionManager;

        public GroupController(
            IGroupService groupService,
            IHubContext<ChatHub> hubContext,
            IUserConnectionManager userConnectionManager)
        {
            _groupService = groupService;
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
        }

        /// <summary>
        /// 创建群聊
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateGroupAsync([FromBody] CreateGroupDto dto)
        {
            var group = await _groupService.CreateGroupAsync(dto);
            if (group == null)
                return BadRequest("创建群聊失败");

            // 查询在线群成员
            var memberIds = dto.MemberIds
                .Where(memberId => memberId > 0 && memberId != dto.OwnerId)
                .Distinct()
                .ToList();

            // 给在线群成员发送创建信息
            foreach (var memberId in memberIds)
            {
                if (_userConnectionManager.TryGetConnection(memberId, out var connectionId))
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("GroupCreated", group);
                }
            }

            return Ok(group);
        }

        [HttpGet("my/{userId:int}")]
        public async Task<IActionResult> GetMyGroupsAsync(int userId)
        {
            if (userId <= 0)
                return BadRequest("用户Id无效");

            var groups = await _groupService.GetMyGroupsAsync(userId);

            return Ok(groups);
        }
        /// <summary>
        /// 获取群历史消息。userId 用于服务端校验访问者是否为群成员。
        /// </summary>
        [HttpGet("{groupId:long}/messages")]
        public async Task<IActionResult> GetGroupMessagesAsync(
            long groupId,
            [FromQuery] int userId)
        {
            if (groupId <= 0 || userId <= 0)
                return BadRequest("群聊或用户 Id 无效");

            var messages = await _groupService.GetGroupMessagesAsync(groupId, userId);

            if (messages == null)
                return Forbid();

            return Ok(messages);
        }
    }
}
