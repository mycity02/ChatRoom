using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatRoom.Client.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IChatService _chatService;

        public int ConversationId { get; set; }
        public int CurrentUserId { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;

        // 好友相关的列表、按钮命令、好友申请回调都放到 FriendPanelViewModel。
        public FriendPanelViewModel FriendPanel { get; }

        // 私聊会话集合，只由主聊天窗口维护。
        public ObservableCollection<PrivateSession> PrivateSessionCollection { get; } = new();

        private PrivateSession? _selectedSession;
        public PrivateSession? SelectedSession
        {
            get => _selectedSession;
            set => SetProperty(ref _selectedSession, value);
        }

        /// <summary>
        /// 初始化 MainViewModel 的构造函数。
        /// </summary>
        /// <param name="chatService">聊天服务，负责 SignalR 连接和消息收发。</param>
        /// <param name="friendPanel">好友面板 ViewModel。</param>
        /// <param name="userId">当前登录用户 id。</param>
        /// <param name="userName">当前登录用户名。</param>
        public MainViewModel(
            IChatService chatService,
            FriendPanelViewModel friendPanel,
            int userId,
            string userName)
        {
            _chatService = chatService;
            FriendPanel = friendPanel;
            CurrentUserId = userId;
            CurrentUserName = userName;

            // 聊天消息和会话列表仍然由主窗口处理。
            _chatService.MessageReceived += OnMessageReceived;
            _chatService.ConversationLoad += OnConversationLoad;

            // 好友申请同意成功后，主窗口负责创建并选中左侧私聊会话。
            FriendPanel.FriendRequestAccepted += OnFriendRequestAccepted;

            InitializeAsync();
        }

        /// <summary>
        /// 初始化方法，连接服务器并注册当前用户。
        /// </summary>
        private async void InitializeAsync()
        {
            await _chatService.ConnectAsync();
            await _chatService.RegisterAsync(CurrentUserId);
        }

        /// <summary>
        /// 好友申请同意成功后，根据服务端返回的 ConversationDto 创建私聊会话。
        /// </summary>
        /// <param name="conversation">服务端返回的私聊会话数据。</param>
        private void OnFriendRequestAccepted(ConversationDto conversation)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var exists = PrivateSessionCollection
                    .FirstOrDefault(session => session.OtherUserId == conversation.OtherUserId);

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

                if (!string.IsNullOrWhiteSpace(conversation.LastMessage))
                {
                    // 服务端同意好友申请时会创建第一条招呼消息。
                    // 这里把返回的 LastMessage 放进当前会话消息列表，点击同意的人可以马上看到。
                    var existsGreeting = session.MessageCollection.Any(message =>
                        message.ConversationId == conversation.ConversationId &&
                        message.Content == conversation.LastMessage &&
                        message.SendTime == conversation.LastMessageTime);

                    if (!existsGreeting)
                    {
                        session.MessageCollection.Add(new ChatMessage
                        {
                            SenderId = CurrentUserId,
                            ReceivedId = conversation.OtherUserId,
                            UserName = CurrentUserName,
                            Content = conversation.LastMessage,
                            ConversationId = conversation.ConversationId,
                            SendTime = conversation.LastMessageTime
                        });
                    }
                }

                PrivateSessionCollection.Add(session);
                SelectedSession = session;
            });
        }

        /// <summary>
        /// 处理收到的私聊消息，并更新对应会话的消息列表和最后一条消息。
        /// </summary>
        /// <param name="chatMessage">服务端推送过来的聊天消息。</param>
        public void OnMessageReceived(ChatMessage chatMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var otherUserId = chatMessage.SenderId == CurrentUserId
                    ? chatMessage.ReceivedId
                    : chatMessage.SenderId;

                if (otherUserId == null)
                    return;

                var session = PrivateSessionCollection
                    .FirstOrDefault(s => s.OtherUserId == otherUserId.Value);

                if (session == null)
                {
                    // 对方刚同意好友申请后，申请人这边可能还没有私聊会话。
                    // 如果此时收到第一条私聊消息，先创建会话，再把消息放进去，避免第一条消息被丢掉。
                    var otherUserName = chatMessage.SenderId == CurrentUserId
                        ? otherUserId.Value.ToString()
                        : chatMessage.UserName;

                    session = new PrivateSession(
                        CurrentUserId,
                        CurrentUserName,
                        otherUserId.Value,
                        otherUserName,
                        _chatService);

                    PrivateSessionCollection.Add(session);
                }

                session.MessageCollection.Add(chatMessage);
                session.LastMessage = chatMessage.Content;
                session.LastMessageTime = chatMessage.SendTime;
            });
        }

        /// <summary>
        /// 加载或刷新当前用户的私聊会话列表。
        /// </summary>
        /// <param name="conversations">服务端返回的会话列表。</param>
        public void OnConversationLoad(List<ConversationDto> conversations)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var selectedOtherUserId = SelectedSession?.OtherUserId;

                foreach (var conversation in conversations)
                {
                    var session = PrivateSessionCollection
                        .FirstOrDefault(session => session.OtherUserId == conversation.OtherUserId);

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

                var removeList = PrivateSessionCollection
                    .Where(s => !conversations.Any(c => c.OtherUserId == s.OtherUserId))
                    .ToList();

                foreach (var session in removeList)
                {
                    PrivateSessionCollection.Remove(session);
                }

                if (selectedOtherUserId != null)
                {
                    SelectedSession = PrivateSessionCollection
                        .FirstOrDefault(session => session.OtherUserId == selectedOtherUserId.Value);
                }

                if (SelectedSession == null)
                {
                    SelectedSession = PrivateSessionCollection.FirstOrDefault();
                }
            });
        }
    }
}
