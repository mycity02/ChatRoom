using ChatRoom.Server.Dto;

namespace ChatRoom.Server.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// 注册
        /// </summary>
        Task<ApiResult> RegisterAsync(string userName, string passWord);

        /// <summary>
        /// 登录
        /// </summary>
        Task<ApiResult<LoginResultData>> LoginAsync(string userName, string passWord);
    }
}
