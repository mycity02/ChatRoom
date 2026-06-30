using ChatRoom.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatRoom.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
