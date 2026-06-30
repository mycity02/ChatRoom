using ChatRoom.Client.Models;
using System;
using System.Collections.Generic;

namespace ChatRoom.Client.Interfaces
{
    public interface IChatService
    {
        // 接收消息事件
        event Action<ChatMessage> MessageReceived;
        // 接收历史消息事件
        event Action<List<ChatMessage>> HistoryMessagesLoad;
        // 连接服务器
        Task ConnectAsync();
        // 发送消息
        Task SendMessageAsync(int userId, string userName, string message);
    }
}
