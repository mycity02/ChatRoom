using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;

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
        public async Task<ConversationDto?> AddFriendAsync(int currentUserId, string friendName)
        {
            FriendDto friendDto = new FriendDto
            {
                CurrentUserId = currentUserId,
                FriendName = friendName
            };
            
            var response = await _httpClient.PostAsJsonAsync("/api/friends/add", friendDto);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ConversationDto>();
        }

        /// <summary>
        /// 好友申请
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="friendName"></param>
        /// <returns></returns>
        public async Task<FriendRequestDto?> AddFriendRequestAsync(int currentUserId, string friendName)
        {
            FriendDto friendDto = new FriendDto
            {
                CurrentUserId = currentUserId,
                FriendName = friendName
            };

            var response = await _httpClient.PostAsJsonAsync("/api/friends/request", friendDto);

            if (!response.IsSuccessStatusCode)
                return null;

            // 服务端会返回刚创建的好友申请信息。
            // 客户端需要这条数据来展示“我发出的申请”。
            return await response.Content.ReadFromJsonAsync<FriendRequestDto>();
        }

        /// <summary>
        /// 同意好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        public async Task<ConversationDto?> AcceptFriendRequestAsync(int friendshipId)
        {
            var response = await _httpClient.PostAsync(
                $"/api/friends/requests/{friendshipId}/accept",
                null);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ConversationDto>();
        }

        /// <summary>
        /// 获取当前用户收到的好友申请。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>别人发给当前用户的好友申请列表。</returns>
        public async Task<List<FriendRequestDto>> GetReceivedFriendRequestsAsync(int userId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<FriendRequestDto>>(
                $"/api/friends/requests/received/{userId}");

            // 接口异常或返回空时给 ViewModel 一个空集合，避免界面初始化时空引用。
            return result ?? new List<FriendRequestDto>();
        }

        /// <summary>
        /// 获取当前用户发出的好友申请。
        /// </summary>
        /// <param name="userId">当前登录用户 Id。</param>
        /// <returns>当前用户发出的好友申请列表。</returns>
        public async Task<List<FriendRequestDto>> GetSentFriendRequestsAsync(int userId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<FriendRequestDto>>(
                $"/api/friends/requests/sent/{userId}");

            // 接口异常或返回空时给 ViewModel 一个空集合，避免界面初始化时空引用。
            return result ?? new List<FriendRequestDto>();
        }

        /// <summary>
        /// 拒绝好友申请
        /// </summary>
        /// <param name="friendshipId"></param>
        /// <returns></returns>
        public async Task<FriendRequestDto?> RejectFriendRequestAsync(int friendshipId)
        {
            var response = await _httpClient.PostAsync(
                $"/api/friends/requests/{friendshipId}/reject",
                null);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<FriendRequestDto>();
        }
    }
}

