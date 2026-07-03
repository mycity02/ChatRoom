using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Models
{
    public class ChatMessage
    {
        // 发送者的Id
        public int  SenderId { get; set; }
        // 发送者的用户名
        public string UserName { get; set; }
        // 消息内容
        public string Content { get; set; }
        // 发送时间
        public DateTime SendTime { get; set; }
        // 会话Id
        public long ConversationId { get; set; }
        // 接收者的Id，如果是群聊则为null
        public int? ReceivedId { get; set; }
    }
}
