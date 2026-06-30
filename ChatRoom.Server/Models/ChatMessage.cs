using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoom.Server.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public int? SenderId { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public DateTime SendTime { get; set; }

        [ForeignKey("SenderId")]
        public User? Sender { get; set; }
    }
}
