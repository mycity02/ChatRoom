namespace ChatRoom.Server.Dto
{
    /// <summary>
    /// 登录/注册请求参数
    /// </summary>
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
