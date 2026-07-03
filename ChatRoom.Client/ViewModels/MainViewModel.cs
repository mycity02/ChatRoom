using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace ChatRoom.Client.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IChatService _chatService;
        private readonly IDialogService _dialogService;

        public int ConversationId { get; set; }
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public ObservableCollection<PrivateSession> PrivateSessionCollection { get; } = new();

        private PrivateSession? _selectedSession;
        public PrivateSession? SelectedSession
        {
            get => _selectedSession;
            set => SetProperty(ref _selectedSession, value);
        }

        // 添加好友命令
        public DelegateCommand AddFriendCommand { get; set; }

        /// <summary>
        /// 初始化 MainViewModel 的构造函数
        /// </summary>
        /// <param name="chatService"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        public MainViewModel(IChatService chatService, 
                            IDialogService dialogService,
                            int userId, string userName)
        {
            _chatService = chatService;
            _dialogService = dialogService;
            CurrentUserId = userId;
            CurrentUserName = userName;
            // 订阅聊天服务的事件
            _chatService.MessageReceived += OnMessageReceived;
            // 订阅会话加载事件
            _chatService.ConversationLoad += OnConversationLoad;
            // 初始化 连接服务器并注册当前用户
            InitializeAsync();

            AddFriendCommand = new DelegateCommand(AddFriend);
        }

        /// <summary>
        /// 显示添加好友对话框，并处理用户输入的好友名称
        /// </summary>
        private void AddFriend()
        {
            _dialogService.ShowDialog("AddFriendDialog", result =>
            {
                if (result.Result != ButtonResult.OK)
                    return;
                var findUserName = result.Parameters.GetValue<string>("FindUserName");

                if (string.IsNullOrWhiteSpace(findUserName))
                    return; 

                MessageBox.Show($"添加好友: {findUserName} 成功");
            });
        }

        /// <summary>
        /// 初始化方法，连接服务器并注册当前用户
        /// </summary>
        private async void InitializeAsync()
        {
            await _chatService.ConnectAsync();
            await _chatService.RegisterAsync(CurrentUserId);
        }

        /// <summary>
        /// 处理会话加载事件，将加载的会话添加到 PrivateSessions 集合中
        /// </summary>
        /// <param name="chatMessage"></param>
        public void OnMessageReceived(ChatMessage chatMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var otherUserId = chatMessage.SenderId == CurrentUserId
                    ? chatMessage.ReceivedId
                    : chatMessage.SenderId;

                if (otherUserId == null)
                    return;

                var session = PrivateSessionCollection.FirstOrDefault(s => s.OtherUserId == otherUserId.Value);

                if (session != null)
                {
                    session.MessageCollection.Add(chatMessage);
                    session.LastMessage = chatMessage.Content;
                    session.LastMessageTime = chatMessage.SendTime;
                }
            });
        }

        /// <summary>
        /// 处理会话加载事件，将加载的会话添加到 PrivateSessions 集合中
        /// </summary>
        /// <param name="conversations"></param>
        public void OnConversationLoad(List<Conversation> conversations)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 清空现有的会话集合
                PrivateSessionCollection.Clear();

                foreach (var conversation in conversations)
                {
                    var session = new PrivateSession
                    (
                        CurrentUserId,
                        CurrentUserName,
                        conversation.OtherUserId,
                        conversation.OtherUserName,
                        _chatService
                    );
                    session.LastMessage = conversation.LastMessage;
                    session.LastMessageTime = conversation.LastMessageTime;

                    PrivateSessionCollection.Add(session);
                }
                SelectedSession ??= PrivateSessionCollection.FirstOrDefault();
            });
        }
    }
}
