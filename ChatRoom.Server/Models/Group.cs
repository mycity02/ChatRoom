using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Server.Models
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int OwnerId { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [ForeignKey(nameof(OwnerId))]
        public User? Owner { get; set; }

        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}