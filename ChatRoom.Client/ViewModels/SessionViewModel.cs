using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace ChatRoom.Client.ViewModels
{
    public abstract class SessionViewModel : BindableBase
    {
        protected readonly int _currentUserId;
        protected readonly string _currentUserName;
        protected readonly IChatService _chatService;

        public abstract string SessionType { get; }
        public abstract string Icon { get; }

        private string _otherUserName = string.Empty;
        public string OtherUserName
        {
            get => _otherUserName;
            set => SetProperty(ref _otherUserName, value);
        }

        private string _lastMessage = string.Empty;
        public string LastMessage
        {
            get => _lastMessage;
            set => SetProperty(ref _lastMessage, value);
        }

        private DateTime _lastMessageTime = DateTime.Now;
        public DateTime LastMessageTime
        {
            get => _lastMessageTime;
            set => SetProperty(ref _lastMessageTime, value);
        }

        public ObservableCollection<ChatMessage> MessageCollection { get; } = new();

        private string _newMessage = string.Empty;
        public string NewMessage
        {
            get => _newMessage;
            set => SetProperty(ref _newMessage, value);
        }

        public DelegateCommand SendCommand { get; }

        protected SessionViewModel(
            int currentUserId,
            string currentUserName,
            string otherUserName,
            IChatService chatService)
        {
            _currentUserId = currentUserId;
            _currentUserName = currentUserName;
            _chatService = chatService;
            OtherUserName = otherUserName;
            SendCommand = new DelegateCommand(Send);
        }

        private async void Send()
        {
            await SendAsync();
        }

        protected abstract Task SendAsync();
    }
}