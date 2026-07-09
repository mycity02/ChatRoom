using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRoom.Client.Dto
{
    public class CreateGroupDto
    {
        // 群主id
        public int OwnerId {  get; set; }
        // 群名
        public string GroupName { get; set; }
        // 群成员列表
        public List<int> MemberIds { get; set; }
    }
}
