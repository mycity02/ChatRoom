using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;

namespace ChatRoom.Client.Services
{
    public class HubCallbackService : IHubCallbackService
    {
        public event Action<ChatMessage>? MessageReceived;
        public event Action<List<ChatMessage>>? HistoryMessagesLoad;
        public event Action<List<ConversationDto>>? ConversationLoad;
        public event Action<FriendRequestDto>? FriendRequestReceived;
        public event Action<FriendRequestDto>? FriendRequestStatusChanged;
        public event Action<long, ChatMessage>? GroupMessageReceived;
        public event Action<GroupDto>? GroupCreated;

        /// <summary>
        /// 统一注册 Hub 服务端主动推送到客户端的所有回调。
        /// ChatService 负责连接和发送，这里只负责接收和分发。
        /// </summary>
        /// <param name="hubConnection">SignalR 连接对象。</param>
        public void RegisterCallbacks(HubConnection hubConnection)
        {
            // 接收群聊消息。服务端传回 userName 和 content，这里转换成客户端统一使用的 ChatMessage。
            hubConnection.On<string, string>("ReceiveMessage", (userName, content) =>
            {
                MessageReceived?.Invoke(new ChatMessage
                {
                    UserName = userName,
                    Content = content,
                    SendTime = DateTime.Now,
                });
            });

            // 接收群聊历史消息。
            hubConnection.On<List<ChatMessage>>("LoadHistory", messages =>
            {
                HistoryMessagesLoad?.Invoke(messages);
            });

            // 接收私聊消息。
            hubConnection.On<ChatMessage>("ReceivePrivateMessage", chatMessage =>
            {
                MessageReceived?.Invoke(chatMessage);
            });

            // 接收会话列表刷新。
            hubConnection.On<List<ConversationDto>>("LoadConversations", conversations =>
            {
                ConversationLoad?.Invoke(conversations);
            });

            // 接收新的好友申请实时通知。
            hubConnection.On<FriendRequestDto>("FriendRequestReceived", request =>
            {
                FriendRequestReceived?.Invoke(request);
            });

            // 接收好友申请状态变化通知，申请人用它更新“发出的申请”。
            hubConnection.On<FriendRequestDto>("FriendRequestStatusChanged", request =>
            {
                FriendRequestStatusChanged?.Invoke(request);
            });

            // 接收群聊消息
            hubConnection.On<long, ChatMessage>(
                "ReceiveGroupMessage", (groupId, chatMessage) =>
                {
                    GroupMessageReceived?.Invoke(groupId, chatMessage);
                });

            hubConnection.On<GroupDto>("GroupCreated", group =>
            {
                GroupCreated?.Invoke(group);
            });
        }
    }
}
