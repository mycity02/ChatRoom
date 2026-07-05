using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatRoom.Client.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IChatService _chatService;
        private readonly IFriendService _friendService;
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
        /// <param name="friendService"></param>
        /// <param name="dialogService"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        public MainViewModel(
            IChatService chatService,
            IFriendService friendService,
            IDialogService dialogService,
            int userId,
            string userName)
        {
            _chatService = chatService;
            _friendService = friendService;
            _dialogService = dialogService;
            CurrentUserId = userId;
            CurrentUserName = userName;

            AddFriendCommand = new DelegateCommand(AddFriend);

            // 订阅聊天服务的事件
            _chatService.MessageReceived += OnMessageReceived;
            // 订阅会话加载事件
            _chatService.ConversationLoad += OnConversationLoad;
            // 初始化 连接服务器并注册当前用户
            InitializeAsync();
        }

        /// <summary>
        /// 显示添加好友对话框，并处理用户输入的好友名称
        /// </summary>
        private void AddFriend()
        {
            _dialogService.ShowDialog("AddFriendDialog", async result =>
            {
                if (result.Result != ButtonResult.OK)
                    return;

                var friendName = result.Parameters.GetValue<string>("friendName");

                if (string.IsNullOrWhiteSpace(friendName))
                    return;

                var conversation = await _friendService.AddFriendAsync(CurrentUserId, friendName);

                if (conversation == null)
                {
                    MessageBox.Show("添加好友失败");
                    return;
                }

                var exists = PrivateSessionCollection
                    .FirstOrDefault(s => s.OtherUserId == conversation.OtherUserId);

                if (exists != null)
                {
                    SelectedSession = exists;
                    return;
                }

                var session = new PrivateSession(
                    CurrentUserId,
                    CurrentUserName,
                    conversation.OtherUserId,
                    conversation.OtherUserName,
                    _chatService);

                session.LastMessage = conversation.LastMessage;
                session.LastMessageTime = conversation.LastMessageTime;

                PrivateSessionCollection.Add(session);
                SelectedSession = session;
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
                // 对方id
                var selectedOtherUserId = SelectedSession?.OtherUserId;

                foreach (var conversation in conversations)
                {
                    // 查询是否已经存在会话
                    var session = PrivateSessionCollection
                    .FirstOrDefault(session => session.OtherUserId == conversation.OtherUserId);

                    // 如果不存在就创建
                    if (session == null)
                    {
                        session = new PrivateSession(
                            CurrentUserId,
                            CurrentUserName,
                            conversation.OtherUserId,
                            conversation.OtherUserName,
                            _chatService);

                        PrivateSessionCollection.Add(session);
                    }

                    session.LastMessage = conversation.LastMessage;
                    session.LastMessageTime = conversation.LastMessageTime;
                }
                

                // 移除不再存在的会话
                var removeList = PrivateSessionCollection
                    .Where(s => !conversations.Any(c => c.OtherUserId == s.OtherUserId))
                    .ToList();

                foreach (var session in removeList)
                {
                    PrivateSessionCollection.Remove(session);
                }

                // 如果刷新前用户已经选中了某个会话，
                // 那刷新后就在新的 PrivateSessionCollection 里，
                //  重新找回这个会话，并继续选中它。
                if (selectedOtherUserId != null)
                {
                    SelectedSession = PrivateSessionCollection
                    .FirstOrDefault(session => session.OtherUserId == selectedOtherUserId.Value);
                }

                // 如果当前没有选中的会话，
                // 就默认选中会话列表里的第一个会话。
                if (SelectedSession == null)
                {
                    SelectedSession = PrivateSessionCollection.FirstOrDefault();
                }
            });
        }
    }
}