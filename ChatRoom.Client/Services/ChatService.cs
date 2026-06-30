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
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State != HubConnectionState.Connected)
                await _hubConnection.StartAsync();
        }

        public async Task SendMessageAsync(int userId, string userName, string message)
        {
            await _hubConnection.InvokeAsync("SendMessage", userId, userName, message);
        }
    }
}
