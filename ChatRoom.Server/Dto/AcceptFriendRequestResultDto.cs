using ChatRoom.Server.Models;

namespace ChatRoom.Server.Dto
{
    public class AcceptFriendRequestResultDto
    {
        /// <summary>
        /// 返回给点击“同意”的用户，用来创建或选中私聊会话。
        /// </summary>
        public ConversationDto Conversation { get; set; } = new();

        /// <summary>
        /// 同意好友申请时自动生成的第一条招呼消息。
        /// </summary>
        public ChatMessage GreetingMessage { get; set; } = new();

        /// <summary>
        /// 状态已经变成 accepted 的好友申请，用来通知申请人更新“发出的申请”。
        /// </summary>
        public FriendRequestDto FriendRequest { get; set; } = new();
    }
}
