using System.Windows;
using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Services;
using ChatRoom.Client.Views;
using ChatRoom.Client.ViewModels;
using Prism.Ioc;

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
            containerRegistry.RegisterSingleton<IFriendService, FriendService>();
            containerRegistry.RegisterSingleton<INavigationService, NavigationService>();

            containerRegistry.RegisterDialog<AddFriendDialog, AddFirendDialogViewModel>();
        }
    }
}
