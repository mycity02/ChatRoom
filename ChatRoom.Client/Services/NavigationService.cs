using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Views;
using ChatRoom.Client.ViewModels;
using System.Windows;

namespace ChatRoom.Client.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IChatService _chatService;
        public NavigationService(IChatService chatService) 
        {
            _chatService = chatService;
        }

        /**
         * ��ת���������
         */
        public void NavigateToChat(int userId, string userName)
        {
            var chatView = new ChatView();
            chatView.DataContext = new ChatViewModel(_chatService, userId, userName);
            chatView.Show();

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
