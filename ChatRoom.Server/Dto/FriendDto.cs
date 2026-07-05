namespace ChatRoom.Server.Dto
{
    public class FriendDto
    {
        // 当前用户的 ID
        public int CurrentUserId { get; set; }
        // 好友的名称
        public string FriendName { get; set; } = string.Empty;
    }
}
