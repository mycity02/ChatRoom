using Prism.Mvvm;

namespace ChatRoom.Client.ViewModels
{
    public class CreateGroupMemberItemViewModel : BindableBase
    {
        private bool _isSelected;

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}