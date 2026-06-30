using ChatRoom.Server.Dto;
using ChatRoom.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatRoom.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 登录
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto dto)
        {
            var result = await _authService.LoginAsync(dto.UserName, dto.Password);
            return Ok(result);
        }

        /// <summary>
        /// 注册
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            var result = await _authService.RegisterAsync(dto.UserName, dto.Password);
            return Ok(result);
        }
    }
}
