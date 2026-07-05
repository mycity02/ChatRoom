using ChatRoom.Server.Dto;
using ChatRoom.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatRoom.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
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
    }
}
