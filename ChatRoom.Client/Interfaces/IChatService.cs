using ChatRoom.Client.Dto;
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

        // 会话加载事件
        event Action<List<ConversationDto>> ConversationLoad;

        // 连接服务器
        Task ConnectAsync();

        // 用户上线注册
        Task RegisterAsync(int userId);

        // 发送群聊消息
        Task SendMessageAsync(int userId, string userName, string message);

        // 发送私聊消息
        Task SendPrivateMessageAsync(int senderId, int receiverId, string senderName, string message);
    }
}
