namespace ChatRoom.Server.Interfaces
{
    public interface IUserConnectionManager
    {
        /// <summary>
        /// 用户上线或者注册sigaIR时，保存UserID和connectionId的关系
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connnectionID"></param>
        void AddConnection(int userId, string connnectionId);

        /// <summary>
        /// 用户断开连接，根据connectionId删除对应记录
        /// </summary>
        /// <param name="connectionId"></param>
        void RemoveConnection(string connectionId);

        /// <summary>
        /// 根据UserId查询当前在线连接状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        bool TryGetConnection(int userId, out string connectionId);
    }
}

