namespace ChatRoom.Server.Dto
{
    public class CreateGroupDto
    {
        public int OwnerId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public List<int> MemberIds { get; set; } = new();
    }
}
