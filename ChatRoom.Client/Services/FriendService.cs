using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using ChatRoom.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ChatRoom.Client.Services
{
    public class FriendService : IFriendService
    {
        // HttpClient 实例用于与服务器进行 HTTP 通信
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        /// <summary>
        /// 添加好友并返回与该好友的会话信息
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="friendName"></param>
        /// <returns></returns>
        public async Task<Conversation?> AddFriendAsync(int currentUserId, string friendName)
        {
            FriendDto friendDto = new FriendDto
            {
                CurrentUserId = currentUserId,
                FriendName = friendName
            };
            
            var response = await _httpClient.PostAsJsonAsync("/api/friends/add", friendDto);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Conversation>();
        }
    }
}
