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
            set
            {
                if (SetProperty(ref _selectedGroupSession, value) && value != null)
                    _ = LoadGroupHistoryAsync(value);
            }
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
                        group.LastMessage,
                        select: false);
                }

                // 批量恢复列表后只选中一个群，避免同时加载每个群的历史消息。
                SelectedGroupSession ??= GroupSessionCollection.FirstOrDefault();
            }
            catch (Exception)
            {
                MessageBox.Show("加载群聊列表失败");
            }
        }

        /// <summary>
        /// 首次选中群聊时加载最近消息。实时消息与历史消息可能交错到达，
        /// 因此按发送者、内容和时间去重后再追加到界面集合。
        /// </summary>
        private async Task LoadGroupHistoryAsync(GroupSessionViewModel session)
        {
            if (session.IsHistoryLoaded)
                return;

            try
            {
                var history = await _groupService.GetGroupMessagesAsync(
                    session.GroupId,
                    _currentUserId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (session.IsHistoryLoaded)
                        return;

                    foreach (var message in history)
                    {
                        var exists = session.MessageCollection.Any(existing =>
                            existing.SenderId == message.SenderId &&
                            existing.Content == message.Content &&
                            existing.SendTime == message.SendTime);

                        if (exists)
                            continue;

                        session.MessageCollection.Add(new ChatMessage
                        {
                            SenderId = message.SenderId,
                            UserName = message.UserName,
                            Content = message.Content,
                            SendTime = message.SendTime
                        });
                    }

                    if (session.MessageCollection.Count > 0)
                        session.LastMessage = session.MessageCollection[^1].Content;

                    session.IsHistoryLoaded = true;
                });
            }
            catch (Exception)
            {
                MessageBox.Show("加载群聊历史消息失败");
            }
        }
        public void AddOrUpdateGroup(
            long groupId,
            string groupName,
            string lastMessage = "",
            bool select = true)
        {
            var exists = GroupSessionCollection
                .FirstOrDefault(group => group.GroupId == groupId);

            if (exists != null)
            {
                exists.GroupName = groupName;
                exists.LastMessage = lastMessage;
                if (select)
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
            if (select)
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
