namespace ChatRoom.Server.Dto
{
    public class MessageDto
    {
        public int SenderId { get; set; }
        public int ReceivedId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long ConversationId { get; set; }
        public DateTime SendTime { get; set; } = DateTime.Now;
    }
}
