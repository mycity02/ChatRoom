using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Models
{
    public class Conversation
    {
        public long ConversationId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
    }
}
