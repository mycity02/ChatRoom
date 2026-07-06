namespace ChatRoom.Server.Dto
{
    public class FriendRequestDto
    {
        /// <summary>
        /// FriendShip 表里的申请记录 Id
        /// </summary>
        public int FriendshipId { get; set; }

        /// <summary>
        /// 发起申请的用户 Id
        /// </summary>
        public int RequestUserId { get; set; }

        /// <summary>
        /// 发起申请的用户名
        /// </summary>
        public string RequestUserName { get; set; } = string.Empty;

        /// <summary>
        /// 接收申请的用户 Id
        /// </summary>
        public int ReceivedUserId { get; set; }

        /// <summary>
        /// 接收申请的用户名
        /// </summary>
        public string ReceivedUserName { get; set; } = string.Empty;

        /// <summary>
        /// 申请状态：pending / accepted / rejected
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 申请创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
