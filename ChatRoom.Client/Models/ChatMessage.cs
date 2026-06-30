using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Models
{
    public class ChatMessage
    {
        // 发送者的用户名
        public string UserName { get; set; }
        // 消息内容
        public string Content { get; set; }
        // 发送时间
        public DateTime SendTime { get; set; }
    }
}
