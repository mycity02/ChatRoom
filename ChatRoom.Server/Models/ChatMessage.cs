using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Server.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int SenderId { get; set; }

        public int? ReceivedId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public long? ConversationId { get; set; }

        public DateTime SendTime { get; set; } = DateTime.Now;

        [ForeignKey(nameof(SenderId))]
        public User? Sender { get; set; }

        [ForeignKey(nameof(ReceivedId))]
        public User? Receiver { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public PrivateConversation? PrivateConversation { get; set; }
    }
}