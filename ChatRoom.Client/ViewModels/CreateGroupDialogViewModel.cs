using ChatRoom.Client.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChatRoom.Client.ViewModels
{
    public class CreateGroupDialogViewModel : BindableBase, IDialogAware
    {
        private string _groupName = string.Empty;

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public ObservableCollection<CreateGroupMemberItemViewModel> FriendCollection { get; } = new();

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public DialogCloseListener RequestClose { get; set; }

        public CreateGroupDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void Confirm()
        {
            var memberIds = FriendCollection
                .Where(friend => friend.IsSelected)
                .Select(friend => friend.UserId)
                .ToList();

            var parameters = new DialogParameters
            {
                { "groupName", GroupName },
                { "memberIds", memberIds }
            };

            RequestClose.Invoke(parameters, ButtonResult.OK);
        }

        private void Cancel()
        {
            RequestClose.Invoke(new DialogParameters(), ButtonResult.Cancel);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var friends = parameters.GetValue<List<FriendItem>>("friends")
                ?? new List<FriendItem>();

            FriendCollection.Clear();

            foreach (var friend in friends)
            {
                FriendCollection.Add(new CreateGroupMemberItemViewModel
                {
                    UserId = friend.Id,
                    UserName = friend.UserName
                });
            }
        }
    }
}