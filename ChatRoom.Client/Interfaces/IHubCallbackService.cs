using ChatRoom.Client.Dto;
using ChatRoom.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;

namespace ChatRoom.Client.Interfaces
{
    public interface IHubCallbackService
    {
        // 接收到聊天消息，包括群聊消息和私聊消息
        event Action<ChatMessage>? MessageReceived;

        // 接收到历史消息列表
        event Action<List<ChatMessage>>? HistoryMessagesLoad;

        // 接收到会话列表刷新
        event Action<List<ConversationDto>>? ConversationLoad;

        // 接收到新的好友申请
        event Action<FriendRequestDto>? FriendRequestReceived;

        // 好友申请状态发生变化，比如 accepted / rejected
        event Action<FriendRequestDto>? FriendRequestStatusChanged;

        /// <summary>
        /// 注册所有 SignalR 服务端回调。
        /// </summary>
        /// <param name="hubConnection">ChatService 创建并维护的 HubConnection。</param>
        void RegisterCallbacks(HubConnection hubConnection);
    }
}
