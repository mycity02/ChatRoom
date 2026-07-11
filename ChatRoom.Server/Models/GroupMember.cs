using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Server.Models
{
    [Index(nameof(GroupId), nameof(UserId), IsUnique = true)]
    public class GroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long GroupId { get; set; }

        public int UserId { get; set; }

        public DateTime JoinTime { get; set; } = DateTime.Now;

        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}