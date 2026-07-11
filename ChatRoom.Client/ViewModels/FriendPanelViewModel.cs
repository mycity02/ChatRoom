using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatRoom.Client.ViewModels
{
    public class FriendPanelViewModel : BindableBase
    {
        private readonly IHubCallbackService _hubCallbackService;
        private readonly IFriendService _friendService;
        private readonly IDialogService _dialogService;
        private readonly int _currentUserId;
        // 选中的好友
        private FriendItem? _selectedFriend;
        // 选中好友时通知
        public event Action<FriendItem>? SelectedFriendChanged;

        // 收到的好友申请，绑定到“收到的申请”列表。
        public ObservableCollection<FriendRequestDto> FriendRequestCollection { get; } = new();

        // 我发出的好友申请，绑定到“发出的申请”列表。
        public ObservableCollection<FriendRequestDto> SentFriendRequestCollection { get; } = new();

        // 好友列表，后续加载好友时继续使用这个集合。
        public ObservableCollection<FriendItem> FriendCollection { get; } = new();

        public DelegateCommand AddFriendRequestCommand { get; }
        public DelegateCommand<FriendRequestDto> AcceptFriendRequestCommand { get; }
        public DelegateCommand<FriendRequestDto> RejectFriendRequestCommand { get; }

        // 同意好友申请后，需要通知 MainViewModel 创建并选中对应私聊会话。
        public event Action<ConversationDto>? FriendRequestAccepted;

        public FriendItem? SelectedFriend
        {
            get => _selectedFriend;
            set
            {
                // 选中好友时通知
                if (SetProperty(ref _selectedFriend, value) && value != null)
                    SelectedFriendChanged?.Invoke(value);
            }
        }

        public FriendPanelViewModel(
            IHubCallbackService hubCallbackService,
            IFriendService friendService,
            IDialogService dialogService,
            int currentUserId)
        {
            _hubCallbackService = hubCallbackService;
            _friendService = friendService;
            _dialogService = dialogService;
            _currentUserId = currentUserId;

            AddFriendRequestCommand = new DelegateCommand(AddFriendRequest);
            AcceptFriendRequestCommand = new DelegateCommand<FriendRequestDto>(AcceptFriendRequest);
            RejectFriendRequestCommand = new DelegateCommand<FriendRequestDto>(RejectFriendRequest);

            // 好友相关 SignalR 回调直接订阅 HubCallbackService，不再经过 ChatService 转发。
            _hubCallbackService.FriendRequestReceived += OnFriendRequestReceived;
            _hubCallbackService.FriendRequestStatusChanged += OnFriendRequestStatusChanged;

            // 进入主界面时，从数据库加载已有的收到/发出的申请。
            InitializeAsync();
        }

        /// <summary>
        /// 初始化好友申请列表。
        /// SignalR 只负责在线实时变化，登录后已有的数据需要从数据库主动加载。
        /// </summary>
        private async void InitializeAsync()
        {
            var receivedRequests = await _friendService.GetReceivedFriendRequestsAsync(_currentUserId);
            var sentRequests = await _friendService.GetSentFriendRequestsAsync(_currentUserId);
            // 获取好友列表
            var getFriendList = await _friendService.GetFriendListAsync(_currentUserId);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // 好友列表
                FriendCollection.Clear();
                foreach(var friend in getFriendList)
                    FriendCollection.Add(friend);

                FriendRequestCollection.Clear();
                foreach (var request in receivedRequests)
                {
                    FriendRequestCollection.Add(request);
                }

                SentFriendRequestCollection.Clear();
                foreach (var request in sentRequests)
                {
                    SentFriendRequestCollection.Add(request);
                }
            });
        }

        /// <summary>
        /// 收到实时好友申请。
        /// </summary>
        /// <param name="request">服务端推送过来的好友申请记录。</param>
        private void OnFriendRequestReceived(FriendRequestDto request)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 防止同一条好友申请被重复添加到界面。
                var exists = FriendRequestCollection
                    .FirstOrDefault(r => r.FriendshipId == request.FriendshipId);

                if (exists != null)
                    return;

                FriendRequestCollection.Add(request);
            });
        }

        /// <summary>
        /// 申请状态变化后，更新“发出的申请”列表。
        /// </summary>
        /// <param name="request">状态已变化的好友申请。</param>
        private void OnFriendRequestStatusChanged(FriendRequestDto request)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var exists = SentFriendRequestCollection
                    .FirstOrDefault(r => r.FriendshipId == request.FriendshipId);

                if (exists != null)
                {
                    SentFriendRequestCollection.Remove(exists);
                }

                SentFriendRequestCollection.Add(request);

                if (request.Status == "accepted")
                {
                    AddFriendIfMissing(
                        request.ReceivedUserId,
                        request.ReceivedUserName);
                }
            });
        }

        /// <summary>
        /// 打开添加好友弹窗，并发送好友申请。
        /// </summary>
        private void AddFriendRequest()
        {
            _dialogService.ShowDialog("AddFriendDialog", async result =>
            {
                if (result.Result != ButtonResult.OK)
                    return;

                var friendName = result.Parameters.GetValue<string>("friendName");

                if (string.IsNullOrWhiteSpace(friendName))
                    return;

                // 这里调用 HTTP 接口创建好友申请。
                // 成功后服务端返回这条申请记录，用来展示在“发出的申请”里。
                var request = await _friendService.AddFriendRequestAsync(_currentUserId, friendName);

                if (request == null)
                {
                    MessageBox.Show("发送好友申请失败");
                    return;
                }

                // 如果列表里已经有同一条申请，先移除旧的，再加入新的。
                var exists = SentFriendRequestCollection
                    .FirstOrDefault(r => r.FriendshipId == request.FriendshipId);

                if (exists != null)
                {
                    SentFriendRequestCollection.Remove(exists);
                }

                SentFriendRequestCollection.Add(request);

                MessageBox.Show("好友申请已发送，等待对方同意");
            });
        }

        /// <summary>
        /// 同意某一条好友申请。
        /// </summary>
        /// <param name="request">按钮所在的那一条好友申请。</param>
        private async void AcceptFriendRequest(FriendRequestDto request)
        {
            if (request == null)
                return;

            // friendshipId 是这条申请记录的主键，不是好友用户 id。
            var conversation = await _friendService.AcceptFriendRequestAsync(request.FriendshipId);

            if (conversation == null)
            {
                MessageBox.Show("同意好友申请失败");
                return;
            }

            // 同意成功后，从“收到的申请”列表里移除这一条。
            var exists = FriendRequestCollection
                .FirstOrDefault(r => r.FriendshipId == request.FriendshipId);

            if (exists != null)
            {
                FriendRequestCollection.Remove(exists);
            }

            AddFriendIfMissing(
                conversation.OtherUserId,
                conversation.OtherUserName);

            // 好友面板不直接操作左侧会话列表，只把结果交给 MainViewModel。
            FriendRequestAccepted?.Invoke(conversation);
        }

        /// <summary>
        /// 拒绝好友申请。
        /// </summary>
        /// <param name="request">按钮所在的那一条好友申请。</param>
        private async void RejectFriendRequest(FriendRequestDto request)
        {
            if (request == null)
                return;

            var result = await _friendService.RejectFriendRequestAsync(request.FriendshipId);

            if (result == null)
            {
                MessageBox.Show("拒绝好友申请失败");
                return;
            }

            FriendRequestCollection.Remove(request);
        }

        private void AddFriendIfMissing(int userId, string userName)
        {
            if (FriendCollection.Any(friend => friend.Id == userId))
                return;

            FriendCollection.Add(new FriendItem
            {
                Id = userId,
                UserName = userName
            });
        }
    }
}



