using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;

namespace ChatRoom.Client.Dto
{
    public class GroupDto
    {
        // 群id
        public long GroupId { get; set; }
        // 群名
        public string GroupName { get; set; } = string.Empty;
        // 群主id
        public int OwnerId { get; set; }
        // 最近一条消息
        public string LastMessage { get; set; } = string.Empty;
        // 建群时间
        public DateTime CreateTime { get; set; }
    }
}
