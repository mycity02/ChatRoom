using ChatRoom.Server.Data;
using ChatRoom.Server.Dto;
using ChatRoom.Server.Interfaces;
using ChatRoom.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResult> RegisterAsync(string userName, string passWord)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == userName))
                return ApiResult.Fail("用户名已存在");

            var user = new User { UserName = userName };
            user.Password = _hasher.HashPassword(user, passWord);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return ApiResult.Ok("注册成功");
        }

        public async Task<ApiResult<LoginResultData>> LoginAsync(string userName, string passWord)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return ApiResult<LoginResultData>.Fail("用户不存在");

            var result = _hasher.VerifyHashedPassword(user, user.Password, passWord);
            if (result == PasswordVerificationResult.Failed)
                return ApiResult<LoginResultData>.Fail("密码错误");

            var data = new LoginResultData
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return ApiResult<LoginResultData>.Ok(data, "登录成功");
        }
    }
}
