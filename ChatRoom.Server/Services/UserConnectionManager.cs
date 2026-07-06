using ChatRoom.Server.Interfaces;
using System.Collections.Concurrent;

namespace ChatRoom.Server.Services
{
    public class UserConnectionManager : IUserConnectionManager
    {
        // 保存用户连接
        private readonly ConcurrentDictionary<int, string> _connections = new();

        /// <summary>
        /// 用户上线或者注册sigaIR时，保存UserID和connectionId的关系
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connnectionId"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddConnection(int userId, string connnectionId)
        {
            _connections[userId] = connnectionId;
        }

        /// <summary>
        /// 用户断开连接，根据connectionId删除对应记录
        /// </summary>
        /// <param name="connectionId"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveConnection(string connectionId)
        {
            // 通过connectionId找到对应用户并删除
            foreach (var pair in _connections.Where(p => p.Value == connectionId).ToList()) 
            {
                _connections.TryRemove(pair.Key, out _);
            }
        }

        /// <summary>
        /// 根据UserId查询当前在线连接状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool TryGetConnection(int userId, out string connectionId)
        {
            return _connections.TryGetValue(userId, out connectionId);
        }
    }
}

