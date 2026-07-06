using ChatRoom.Client.Interfaces;
using ChatRoom.Client.ViewModels;
using ChatRoom.Client.Views;
using Prism.Dialogs;
using System.Windows;

namespace ChatRoom.Client.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IChatService _chatService;
        private readonly IHubCallbackService _hubCallbackService;
        private readonly IFriendService _friendService;
        private readonly IDialogService _dialogService;

        public NavigationService(
            IChatService chatService,
            IHubCallbackService hubCallbackService,
            IFriendService friendService,
            IDialogService dialogService)
        {
            _chatService = chatService;
            _hubCallbackService = hubCallbackService;
            _friendService = friendService;
            _dialogService = dialogService;
        }

        /// <summary>
        /// 跳转到聊天界面。
        /// </summary>
        /// <param name="userId">当前登录用户 id。</param>
        /// <param name="userName">当前登录用户名。</param>
        public void NavigateToChat(int userId, string userName)
        {
            ShowMainView(userId, userName);
        }

        /// <summary>
        /// 跳转到主界面。
        /// </summary>
        /// <param name="userId">当前登录用户 id。</param>
        /// <param name="userName">当前登录用户名。</param>
        public void NavigateToMainView(int userId, string userName)
        {
            ShowMainView(userId, userName);
        }

        private void ShowMainView(int userId, string userName)
        {
            var mainView = new MainView();

            // 好友面板单独创建，好友实时回调直接来自 HubCallbackService。
            var friendPanelViewModel = new FriendPanelViewModel(
                _hubCallbackService,
                _friendService,
                _dialogService,
                userId);

            mainView.DataContext = new MainViewModel(
                _chatService,
                friendPanelViewModel,
                userId,
                userName);

            mainView.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginView)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
