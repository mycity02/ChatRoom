namespace ChatRoom.Client.Interfaces
{
    public interface INavigationService
    {
        /// <summary>
        /// 跳转到聊天界面
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        void NavigateToChat(int userId, string userName);

        /// <summary>
        /// 跳转到主界面
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        void NavigateToMainView(int userId, string userName);
    }
}
