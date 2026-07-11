using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatRoom.Client.ViewModels
{
    public class GroupViewModel : BindableBase
    {
        private readonly int _currentUserId;
        private readonly string _currentUserName;
        private readonly ObservableCollection<FriendItem> _friendCollection;
        private readonly IDialogService _dialogService;
        private readonly IGroupService _groupService;
        private readonly IChatService _chatService;

        private GroupSessionViewModel? _selectedGroupSession;

        public ObservableCollection<GroupSessionViewModel> GroupSessionCollection { get; } = new();

        public GroupSessionViewModel? SelectedGroupSession
        {
            get => _selectedGroupSession;
            set => SetProperty(ref _selectedGroupSession, value);
        }

        public DelegateCommand CreateGroupCommand { get; }

        public GroupViewModel(
            int currentUserId,
            string currentUserName,
            ObservableCollection<FriendItem> friendCollection,
            IDialogService dialogService,
            IGroupService groupService,
            IChatService chatService
            )
        {
            _currentUserId = currentUserId;
            _currentUserName = currentUserName;
            _friendCollection = friendCollection;
            _dialogService = dialogService;
            _groupService = groupService;
            _chatService = chatService;

            CreateGroupCommand = new DelegateCommand(CreateGroup);
            _ = LoadGroupsAsync();
        }

        /// <summary>
        /// 创建群聊
        /// </summary>
        private void CreateGroup()
        {
            var parameters = new DialogParameters
            {
                { "friends", _friendCollection.ToList() }
            };

            _dialogService.ShowDialog("CreateGroupDialog", parameters, async result =>
            {
                if (result.Result != ButtonResult.OK)
                    return;

                var groupName = result.Parameters.GetValue<string>("groupName");
                var memberIds = result.Parameters.GetValue<List<int>>("memberIds");

                if (string.IsNullOrWhiteSpace(groupName))
                    return;

                var group = await _groupService.CreateGroupAsync(
                    _currentUserId, groupName.Trim(), memberIds);

                if (group == null)
                {
                    MessageBox.Show("创建群聊失败");
                    return;
                }

                AddOrUpdateGroup(group.GroupId, group.GroupName, group.LastMessage);
            });
        }

        /// <summary>
        /// 加载群聊列表
        /// </summary>
        /// <returns></returns>
        private async Task LoadGroupsAsync()
        {
            try
            {
                var groups = await _groupService.GetMyGroupAsync(_currentUserId);

                foreach (var group in groups)
                {
                    AddOrUpdateGroup(
                        group.GroupId,
                        group.GroupName,
                        group.LastMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载群聊列表失败");
            }
        }

        public void AddOrUpdateGroup(long groupId, string groupName, string lastMessage = "")
        {
            var exists = GroupSessionCollection
                .FirstOrDefault(group => group.GroupId == groupId);

            if (exists != null)
            {
                exists.GroupName = groupName;
                exists.LastMessage = lastMessage;
                SelectedGroupSession = exists;
                return;
            }

            var session = new GroupSessionViewModel(
                _currentUserId,
                _currentUserName,
                groupId,
                groupName,
                _chatService);

            session.LastMessage = lastMessage;
            GroupSessionCollection.Add(session);
            SelectedGroupSession = session;
        }

        /// <summary>
        /// 接收群聊消息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="message"></param>
        public void ReceiveGroupMessage(long groupId, ChatMessage message)
        {
            var session = GroupSessionCollection
                .FirstOrDefault(group => group.GroupId == groupId);

            if (session == null)
                return;

            session.MessageCollection.Add(message);
            session.LastMessage = message.Content;
        }
    }
}