namespace ChatRoom.Server.Dto
{
    /// <summary>
    /// 返回给客户端的群聊消息，避免直接暴露 EF 实体及其导航属性。
    /// </summary>
    public class GroupMessageDto
    {
        public int SenderId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SendTime { get; set; }
    }
}