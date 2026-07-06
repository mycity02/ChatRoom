using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Models
{
    public class FriendItem
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Icon => string.IsNullOrWhiteSpace(UserName)
            ? "?" : UserName[0].ToString();
    }
}
