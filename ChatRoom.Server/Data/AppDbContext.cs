using ChatRoom.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        // 用户表
        public DbSet<User> Users { get; set; }
        // 聊天记录表
        public DbSet<ChatMessage> ChatMessages { get; set; }
        // 私聊会话表
        public DbSet<PrivateConversation> PrivateConversations { get; set; }
        // 联系人表
        public DbSet<FriendShip> Friendships { get; set; }
    }
}