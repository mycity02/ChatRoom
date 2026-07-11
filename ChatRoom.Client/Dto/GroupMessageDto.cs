namespace ChatRoom.Client.Dto
{
    /// <summary>
    /// 与服务端群历史消息接口对应的数据结构。
    /// </summary>
    public class GroupMessageDto
    {
        public int SenderId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SendTime { get; set; }
    }
}