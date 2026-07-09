using ChatRoom.Client.Interfaces;

namespace ChatRoom.Client.ViewModels
{
    public class PrivateSessionViewModel : SessionViewModel
    {
        public override string SessionType => "Private";
        public override string Icon => OtherUserName.Length > 0 ? OtherUserName[0].ToString() : "?";

        public int OtherUserId { get; set; }

        public PrivateSessionViewModel(
            int currentUserId,
            string currentUserName,
            int otherUserId,
            string otherUserName,
            IChatService chatService)
            : base(currentUserId, currentUserName, otherUserName, chatService)
        {
            OtherUserId = otherUserId;
        }

        protected override async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _chatService.SendPrivateMessageAsync(
                _currentUserId,
                OtherUserId,
                _currentUserName,
                NewMessage);

            NewMessage = string.Empty;
        }
    }
}