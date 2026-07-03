using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Server.Models
{
    public class PrivateConversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ConversationId { get; set; }

        public int User1Id { get; set; }

        public int User2Id { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [ForeignKey(nameof(User1Id))]
        public User? User1 { get; set; }

        [ForeignKey(nameof(User2Id))]
        public User? User2 { get; set; }

        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}