using ChatRoom.Server.Data;
using ChatRoom.Server.Hubs;
using ChatRoom.Server.Interfaces;
using ChatRoom.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 注册 SignalR 服务
builder.Services.AddSignalR();

// 注册自定义服务
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IFriendService, FriendService>();

// 注册 EF Core + MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

// 暴露 /chathub 端点
app.MapHub<ChatHub>("/chathub");         

app.Run();
