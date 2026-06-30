using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatRoom.Client.ViewModels
{
    public class ChatViewModel : BindableBase
    {
        private readonly IChatService _chatService;
        private readonly int _userId;
        private bool _isConnected;
        private string _userName;
        private string _message;
        private ObservableCollection<ChatMessage> _messageCollection;

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand SendCommand { get; set; }

        public ChatViewModel(IChatService chatService, int userId, string userName)
        {
            _chatService = chatService;
            _userId = userId;
            _userName = userName;

            // 连接命令
            ConnectCommand = new DelegateCommand(async () =>
            {
                await _chatService.ConnectAsync();
                IsConnected = true;
            });

            // 发送消息
            SendCommand = new DelegateCommand(async () =>
            {
                if (string.IsNullOrWhiteSpace(Message)) return;
                await _chatService.SendMessageAsync(_userId, UserName, Message);
                Message = "";
            });

            // 初始化消息集合
            _messageCollection = new ObservableCollection<ChatMessage>();

            // 订阅接收消息事件
            _chatService.MessageReceived += msg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _messageCollection.Add(msg);
                });
            };

            // 订阅接收历史消息事件
            _chatService.HistoryMessagesLoad += msgList =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _messageCollection.Clear();
                    foreach (var msg in msgList)
                        _messageCollection.Add(msg);
                });
            };
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public ObservableCollection<ChatMessage> MessageCollection => _messageCollection;
    }
}
