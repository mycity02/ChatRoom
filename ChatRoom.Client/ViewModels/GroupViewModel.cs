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
            IGroupService groupService
            )
        {
            _currentUserId = currentUserId;
            _currentUserName = currentUserName;
            _friendCollection = friendCollection;
            _dialogService = dialogService;
            _groupService = groupService;

            CreateGroupCommand = new DelegateCommand(CreateGroup);
        }

        /// <summary>
        /// 创建群聊
        /// </summary>
        private void CreateGroup()
        {
            var parameters = new DialogParameters
            {
                { "friends", _friendCollection }
            };

            _dialogService.ShowDialog("CreateGroupDialog", async result =>
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
                groupName);

            session.LastMessage = lastMessage;
            GroupSessionCollection.Add(session);
            SelectedGroupSession = session;
        }

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