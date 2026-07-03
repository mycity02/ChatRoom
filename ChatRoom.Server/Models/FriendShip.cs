using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Server.Models
{
    public class FriendShip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // 好友列表id
        public int Id { get; set; }
        // 请求者id
        public int RequestId { get; set; }
        // 接收者id
        public int ReceivedId { get; set; }
        // 好友状态
        public string Status { get; set; } = "pending";
        // 添加时间
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 请求者信息
        [ForeignKey(nameof(RequestId))]
        public User Requester { get; set; }
        // 接受者信息
        [ForeignKey(nameof(ReceivedId))]
        public User Receiver { get; set; }
    }
}
