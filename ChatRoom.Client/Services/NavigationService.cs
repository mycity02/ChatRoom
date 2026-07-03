using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Views;
using ChatRoom.Client.ViewModels;
using System.Windows;

namespace ChatRoom.Client.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IChatService _chatService;
        private readonly IDialogService _dialogService;
        public NavigationService(IChatService chatService, IDialogService dialogService)
        {
            _chatService = chatService;
            _dialogService = dialogService;
        }

        /// <summary>
        /// 跳转到聊天界面
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        public void NavigateToChat(int userId, string userName)
        {
            var mainView = new MainView();
            mainView.DataContext = new MainViewModel(_chatService, _dialogService, userId, userName);
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

        /// <summary>
        /// 跳转到主界面
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        public void NavigateToMainView(int userId, string userName)
        {
            var mainView = new MainView();
            mainView.DataContext = new MainViewModel(_chatService, _dialogService, userId, userName);
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
