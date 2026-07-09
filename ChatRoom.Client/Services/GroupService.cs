using ChatRoom.Client.Dto;
using ChatRoom.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace ChatRoom.Client.Services
{
    public class GroupService : IGroupService
    {
        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        /// <summary>
        /// 创建群聊
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="groupName"></param>
        /// <param name="membersId"></param>
        /// <returns></returns>
        public async Task<GroupDto?> CreateGroupAsync(int ownerId, string groupName, List<int> membersId)
        {
            var dto = new CreateGroupDto
            {
                OwnerId = ownerId,
                GroupName = groupName,
                MemberIds = membersId
            };

            var response = await _httpClient.PostAsJsonAsync("api/groups/create", dto);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<GroupDto>();
        }

        /// <summary>
        /// 获取我加入的群聊
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<GroupDto>> GetMyGroupAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<List<GroupDto>>($"/api/groups/my/{userId}")
                ?? new List<GroupDto>();
        }
    }
}
