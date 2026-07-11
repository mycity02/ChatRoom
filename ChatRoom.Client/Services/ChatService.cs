using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;

namespace ChatRoom.Client.Services
{
    public class ChatService : IChatService
    {
        private readonly HubConnection _hubConnection;
        private readonly IHubCallbackService _hubCallbackService;

        public event Action<ChatMessage> MessageReceived;
        public event Action<List<ChatMessage>> HistoryMessagesLoad;
        public event Action<List<ConversationDto>> ConversationLoad;
        public event Action<long, ChatMessage> GroupMessageReceived;
        public event Action<GroupDto> GroupCreated;

        public ChatService(IHubCallbackService hubCallbackService)
        {
            _hubCallbackService = hubCallbackService;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chatHub")
                .Build();

            // ChatService 只转发聊天和会话相关事件。
            // 好友相关 SignalR 回调由 FriendPanelViewModel 直接订阅 HubCallbackService。
            _hubCallbackService.MessageReceived += chatMessage => MessageReceived?.Invoke(chatMessage);
            _hubCallbackService.HistoryMessagesLoad += messages => HistoryMessagesLoad?.Invoke(messages);
            _hubCallbackService.ConversationLoad += conversations => ConversationLoad?.Invoke(conversations);

           
            _hubCallbackService.GroupMessageReceived += (groupId, chatMessage) => 
                GroupMessageReceived?.Invoke(groupId, chatMessage);

            _hubCallbackService.GroupCreated += group =>
                GroupCreated?.Invoke(group);

            // 所有 _hubConnection.On(...) 都集中放在 HubCallbackService 里注册。
            _hubCallbackService.RegisterCallbacks(_hubConnection);
        }

        /// <summary>
        /// 连接到 SignalR Hub。
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (_hubConnection.State != HubConnectionState.Connected)
                await _hubConnection.StartAsync();
        }

        /// <summary>
        /// 发送群聊消息到 SignalR Hub。
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(int userId, string userName, string message)
        {
            await _hubConnection.InvokeAsync("SendMessage", userId, userName, message);
        }

        /// <summary>
        /// 注册当前用户到 SignalR Hub。
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RegisterAsync(int userId)
        {
            await _hubConnection.InvokeAsync("RegisterUser", userId);
        }

        /// <summary>
        /// 发送私聊消息到 SignalR Hub。
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <param name="senderName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendPrivateMessageAsync(int senderId, int receiverId, string senderName, string message)
        {
            await _hubConnection.InvokeAsync("SendPrivateMessage", senderId, receiverId, senderName, message);
        }

        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="senderId"></param>
        /// <param name="senderName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendGroupMessageAsync(long groupId, int senderId, string senderName, string message)
        {
            await _hubConnection.InvokeAsync("SendGroupMessage", groupId,
                senderId, senderName, message);
        }
    }
}
