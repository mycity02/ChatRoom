using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace ChatRoom.Client.ViewModels
{
    public class GroupSessionViewModel : BindableBase
    {
        private readonly int _currentUserId;
        private readonly string _currentUserName;
        private readonly IChatService _chatService;

        private string _groupName = string.Empty;
        private string _lastMessage = string.Empty;
        private string _newMessage = string.Empty;

        public long GroupId { get; }

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (SetProperty(ref _groupName, value))
                    RaisePropertyChanged(nameof(Icon));
            }
        }

        public string Icon => string.IsNullOrWhiteSpace(GroupName)
            ? "群"
            : GroupName[0].ToString();

        public string LastMessage
        {
            get => _lastMessage;
            set => SetProperty(ref _lastMessage, value);
        }

        public ObservableCollection<ChatMessage> MessageCollection { get; } = new();

        // 历史消息只在首次选中该群时加载，避免重复追加同一批数据。
        public bool IsHistoryLoaded { get; set; }

        public string NewMessage
        {
            get => _newMessage;
            set => SetProperty(ref _newMessage, value);
        }

        public DelegateCommand SendCommand { get; }

        public GroupSessionViewModel(
            int currentUserId,
            string currentUserName,
            long groupId,
            string groupName,
            IChatService chatService)
        {
            _currentUserId = currentUserId;
            _currentUserName = currentUserName;
            _chatService = chatService;
            GroupId = groupId;
            GroupName = groupName;
            SendCommand = new DelegateCommand(Send);
        }

        private async void Send()
        {
            await SendAsync();
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <returns></returns>
        private async Task SendAsync()
        {
            var message = NewMessage.Trim();

            if (string.IsNullOrWhiteSpace(message))
                return;

            await _chatService.SendGroupMessageAsync(
                GroupId,
                _currentUserId,
                _currentUserName,
                message);

            NewMessage = string.Empty;
        }
    }
}