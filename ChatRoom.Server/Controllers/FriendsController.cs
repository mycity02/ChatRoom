using ChatRoom.Server.Dto;
using ChatRoom.Server.Hubs;
using ChatRoom.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatRoom.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUserConnectionManager _userConnectionManager;

        public FriendsController(
            IFriendService friendService,
            IHubContext<ChatHub> hubContext,
            IUserConnectionManager userConnectionManager)
        {
            _friendService = friendService;
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
        }

        /// <summary>
        /// 添加好友并创建会话
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddFriend([FromBody] FriendDto friendDto)
        {
            var result = await _friendService.AddFriendAsync(friendDto);

            if (result == null)
                return BadRequest("添加好友失败");

            return Ok(result);
        }

        /// <summary>
        /// 添加好友申请
        /// </summary>
        /// <param name="friendDto"></param>
        /// <returns></returns>
        [HttpPost("request")]
        public async Task<IActionResult> AddFriendRequestAsync([FromBody] FriendDto friendDto)
        {
            // 写入数据库
            var result = await _friendService.AddFriendRequestAsync(friendDto);

            if (result == null)
                return BadRequest("发送好友申请失败");

            // 如果用户在线就实时推送给他
            if (_userConnectionManager.TryGetConnection(result.ReceivedUserId, out var connectionId))
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("FriendRequestReceived", result);
            }

            return Ok(result);
        }
        /// <summary>
        /// 获取当前用户收到的好友申请。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>待当前用户处理的好友申请。</returns>
        [HttpGet("requests/received/{userId}")]
        public async Task<IActionResult> GetReceivedFriendRequestsAsync(int userId)
        {
            var result = await _friendService.GetReceivedFriendRequestsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// 获取当前用户发出的好友申请。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>当前用户发出的好友申请。</returns>
        [HttpGet("requests/sent/{userId}")]
        public async Task<IActionResult> GetSentFriendRequestsAsync(int userId)
        {
            var result = await _friendService.GetSentFriendRequestsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// 同意好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        [HttpPost("requests/{friendshipId}/accept")]
        public async Task<IActionResult> AcceptFriendRequestAsync(int friendshipId)
        {
            var result = await _friendService.AcceptAddFriendRequestAsync(friendshipId);

            if (result == null)
                return BadRequest("同意好友申请失败");

            var greetingMessage = result.GreetingMessage;
            var messageDto = new
            {
                SenderId = greetingMessage.SenderId,
                ReceivedId = greetingMessage.ReceivedId,
                UserName = greetingMessage.UserName,
                Content = greetingMessage.Content,
                ConversationId = greetingMessage.ConversationId,
                SendTime = greetingMessage.SendTime
            };

            // 通知申请人：好友申请已经被同意，更新“发出的申请”状态。
            if (_userConnectionManager.TryGetConnection(result.FriendRequest.RequestUserId, out var requestConnectionId))
            {
                await _hubContext.Clients.Client(requestConnectionId)
                    .SendAsync("FriendRequestStatusChanged", result.FriendRequest);

                // 同意后由服务端主动发第一条招呼消息，申请人在线时可以马上看到。
                await _hubContext.Clients.Client(requestConnectionId)
                    .SendAsync("ReceivePrivateMessage", messageDto);
            }

            // HTTP 仍然只返回 ConversationDto，客户端原来的 AcceptFriendRequestAsync 不需要换返回类型。
            return Ok(result.Conversation);
        }

        /// <summary>
        /// 拒绝好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        [HttpPost("requests/{friendshipId}/reject")]
        public async Task<IActionResult> RejectFriendRequestAsync(int friendshipId)
        {
            var result = await _friendService.RejectAddFriendRequestAsync(friendshipId);

            if (result == null)
                return BadRequest("拒绝好友申请失败");

            // 通知申请人
            if (_userConnectionManager.TryGetConnection(result.RequestUserId, out var connectionId))
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("FriendRequestStatusChanged", result);
            }

            return Ok(result);
        }
    }
}




