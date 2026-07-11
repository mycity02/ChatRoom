namespace ChatRoom.Server.Dto
{
    public class GroupDto
    {
        public long GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int OwnerId { get; set; }

        public string LastMessage { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; }
    }
}
