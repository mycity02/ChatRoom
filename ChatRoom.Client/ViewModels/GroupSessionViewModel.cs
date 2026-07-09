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
            string groupName)
        {
            _currentUserId = currentUserId;
            _currentUserName = currentUserName;
            GroupId = groupId;
            GroupName = groupName;
            SendCommand = new DelegateCommand(Send);
        }

        private async void Send()
        {
            await SendAsync();
        }

        private async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
                return;

            // 后续接入 IChatService.SendGroupMessageAsync(...)
            NewMessage = string.Empty;

            await Task.CompletedTask;
        }
    }
}