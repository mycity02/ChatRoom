using ChatRoom.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace ChatRoom.Client.Models
{
    public abstract class Session : BindableBase
    {
        protected readonly int _currentUserId;
        protected readonly string _currentUserName;
        protected readonly IChatService _chatService;

        public abstract string SessionType { get; }
        public abstract string Icon { get; }

        // 显示名称
        private string _otherUserName = string.Empty;
        public string OtherUserName
        {
            get => _otherUserName;
            set => SetProperty(ref _otherUserName, value);
        }

        // 显示最后一条消息
        private string _lastMessage = string.Empty;
        public string LastMessage
        {
            get => _lastMessage;
            set => SetProperty(ref _lastMessage, value);
        }

        // 显示最后一条消息的时间
        private DateTime _lastMessageTime = DateTime.Now;
        public DateTime LastMessageTime
        {
            get => _lastMessageTime;
            set => SetProperty(ref _lastMessageTime, value);
        }

        // 显示未读消息数量
        public ObservableCollection<ChatMessage> MessageCollection { get; } = new();
        private string _newMessage = string.Empty;
        public string NewMessage
        {
            get => _newMessage;
            set => SetProperty(ref _newMessage, value);
        }

        // 发送消息命令
        public DelegateCommand SendCommand { get; set; }
        protected Session(int currentUserId, string currentUserName, string otherUserName, IChatService chatService)
        {
            _currentUserId = currentUserId;
            _currentUserName = currentUserName;
            _chatService = chatService;
            OtherUserName = otherUserName;
            SendCommand = new DelegateCommand(async () => await SendAsync());
        } 

        protected abstract Task SendAsync();
    }
}
