using ChatRoom.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Models
{
    public class PrivateSession : Session
    {
        public override string SessionType =>  "Private";
        public override string Icon => OtherUserName.Length > 0 ? OtherUserName[0].ToString() : "?";
        // 另一方用户的Id
        public int OtherUserId { get; set; }
        public PrivateSession(int currentUserId, string currentUserName,
            int otherUserId, string otherUserName, IChatService chatService) 
            : base(currentUserId, currentUserName, otherUserName, chatService)
        {
            OtherUserId = otherUserId;
        }

        /**
         * 发送消息
         */
        protected override async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _chatService.SendPrivateMessageAsync(_currentUserId, OtherUserId, _currentUserName, NewMessage);

            NewMessage = string.Empty;
        }
    }
}
