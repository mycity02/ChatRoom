using ChatRoom.Server.Data;
using ChatRoom.Server.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _dbContext;

        public ChatHub(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendMessage(int userId, string userName, string message)
        {
            // 将消息保存到数据库
            var chatMessage = new ChatMessage
            {
                SenderId = userId,
                UserName = userName,
                Content = message,
                SendTime = DateTime.Now
            };
            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

            // 广播消息给所有连接的客户端
            await Clients.All.SendAsync("ReceiveMessage", userName, message);
        }

        /// <summary>
        /// 当客户端连接到Hub时触发
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // 获取最近50条消息
            var recentMessages = await _dbContext.ChatMessages
                .OrderByDescending(m => m.SendTime)
                .Take(50)
                .OrderBy(m => m.SendTime)
                .ToListAsync();

            // 将最近的消息发送给连接的客户端
            await Clients.Caller.SendAsync("LoadHistory", recentMessages);

            // 调用基类的OnConnectedAsync方法
            await base.OnConnectedAsync();
        }
    }
}
