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
        public event Action<ChatMessage> MessageReceived;
        public event Action<List<ChatMessage>> HistoryMessagesLoad;
        public event Action<List<Conversation>> ConversationLoad;

        public ChatService()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chatHub")
                .Build();

            // 订阅接收消息事件
            _hubConnection.On<string, string>("ReceiveMessage", (userName, content) =>
            {
                MessageReceived?.Invoke(new ChatMessage
                {
                    UserName = userName,
                    Content = content,
                    SendTime = DateTime.Now,
                });
            });

            // 订阅接收历史消息事件
            _hubConnection.On<List<ChatMessage>>("LoadHistory", (messages) =>
            {
                HistoryMessagesLoad?.Invoke(messages);
            });

            // 一对一聊天接收消息事件
            _hubConnection.On<ChatMessage> ("ReceivePrivateMessage", (chatMessage) =>
            {
                MessageReceived?.Invoke(chatMessage);
            });

            // 会话列表加载
            _hubConnection.On<List<Conversation>>("LoadConversations", (conversations) =>
            {
                ConversationLoad?.Invoke(conversations);
            });
        }

        /// <summary>
        /// 连接到 SignalR Hub
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (_hubConnection.State != HubConnectionState.Connected)
                await _hubConnection.StartAsync();
        }

        /// <summary>
        /// 发送消息到 SignalR Hub
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
        /// 注册用户到 SignalR Hub
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <param name="senderName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RegisterAsync(int userId)
        {
            await _hubConnection.InvokeAsync("RegisterUser", userId);
        }

        /// <summary>
        /// 发送私聊消息到 SignalR Hub
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
    }
}
