using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Services;
using ChatRoom.Client.ViewModels;
using ChatRoom.Client.Views;
using Prism.Ioc;
using System.Windows;

namespace ChatRoom.Client
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<LoginView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IChatService, ChatService>();
            containerRegistry.RegisterSingleton<INavigationService, NavigationService>();
            containerRegistry.RegisterDialog<AddFriendDialog, AddFirendDialogViewModel>();
        }
    }
}
