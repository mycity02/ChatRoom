namespace ChatRoom.Server.Dto
{
    /// <summary>
    /// 登录成功返回的用户数据
    /// </summary>
    public class LoginResultData
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
