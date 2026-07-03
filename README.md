# ChatRoom - 实时聊天室

基于 **ASP.NET Core + SignalR + WPF (Prism MVVM)** 的实时一对一聊天应用。

## 技术栈

| 层级 | 技术 |
|------|------|
| 服务端 | ASP.NET Core (.NET 10) + SignalR + EF Core + MySQL |
| 客户端 | WPF (.NET 10) + Prism + SignalR Client |
| 数据库 | MySQL |
| ORM | EF Core (Pomelo.EntityFrameworkCore.MySql) |
| 认证 | HTTP API + ASP.NET Core Identity PasswordHasher |
| 实时通信 | SignalR WebSocket（自动降级） |
| MVVM | Prism (BindableBase + DelegateCommand + DI) |

## 项目结构

```
ChatRoom/
├── ChatRoom.Server/                  # 服务端
│   ├── Controllers/
│   │   └── AuthController.cs         # 登录/注册 API
│   ├── Hubs/
│   │   └── ChatHub.cs               # SignalR 消息中心（私聊、会话管理）
│   ├── Models/
│   │   ├── User.cs                  # 用户实体
│   │   ├── ChatMessage.cs           # 消息实体
│   │   ├── PrivateConversation.cs   # 私聊会话实体
│   │   └── FriendShip.cs            # 好友关系实体
│   ├── Dto/
│   │   ├── UserDto.cs               # 请求参数
│   │   ├── ApiResult.cs             # 统一响应格式
│   │   └── LoginResultData.cs       # 登录响应数据
│   ├── Interfaces/
│   │   └── IAuthService.cs          # 认证服务接口
│   ├── Services/
│   │   └── AuthService.cs           # 认证服务实现
│   ├── Data/
│   │   └── AppDbContext.cs          # EF Core 数据库上下文
│   ├── Migrations/                  # 数据库迁移文件
│   ├── Program.cs                   # 启动入口
│   └── appsettings.json             # 配置文件（连接字符串）
│
└── ChatRoom.Client/                  # WPF 客户端
    ├── Views/
    │   ├── LoginView.xaml            # 登录/注册界面
    │   ├── MainView.xaml             # 主界面（会话列表 + 聊天区）
    │   └── ChatView.xaml             # 单聊界面
    ├── ViewModels/
    │   ├── LoginViewModel.cs         # 登录/注册逻辑
    │   ├── MainViewModel.cs          # 主界面逻辑（会话管理）
    │   └── ChatViewModel.cs          # 聊天逻辑
    ├── Models/
    │   ├── ChatMessage.cs            # 消息模型
    │   ├── Session.cs                # 会话基类
    │   ├── PrivateSession.cs         # 私聊会话
    │   └── Conversation.cs           # 会话列表模型
    ├── Services/
    │   ├── ChatService.cs            # SignalR 通信服务
    │   └── NavigationService.cs      # 页面导航服务
    ├── Interfaces/
    │   ├── IChatService.cs           # 通信接口
    │   └── INavigationService.cs     # 导航接口
    └── App.xaml.cs                   # Prism 启动配置
```

## 功能

- ✅ 用户注册/登录（HTTP API + 密码哈希）
- ✅ 一对一实时私聊（SignalR）
- ✅ 会话列表（自动加载所有私聊会话及最后一条消息）
- ✅ 好友系统（添加好友、好友状态管理）
- ✅ 消息持久化到 MySQL（含发送者/接收者/会话关联）
- ✅ 历史消息加载（切换会话时加载对应聊天记录）
- ✅ 消息实时推送（双方同时收到）

## 运行

### 1. 配置数据库

编辑 `ChatRoom.Server/appsettings.json`，修改 MySQL 连接字符串：

```json
"ConnectionStrings": {
  "Default": "server=你的服务器;port=3306;database=ChatRoom;user=用户名;password=密码"
}
```

### 2. 创建数据库

```sql
CREATE DATABASE ChatRoom CHARACTER SET utf8mb4;
```

### 3. 执行数据库迁移

```bash
cd ChatRoom.Server
dotnet ef database update
```

### 4. 启动服务端

```bash
cd ChatRoom.Server
dotnet run
```

服务端启动后监听 `http://localhost:5000`。

### 5. 启动客户端

在 Visual Studio 中运行 `ChatRoom.Client`，或：

```bash
dotnet run --project ChatRoom.Client
```

## 消息流程

```
客户端A                     服务端                      客户端B
  │                          │                          │
  ├─ RegisterUser(userId) ──>│                          │
  │<── LoadConversations ────│  （返回会话列表）          │
  │                          │                          │
  ├─ SendPrivateMessage() ──>│                          │
  │                          ├─ 查找/创建私聊会话         │
  │                          ├─ 保存消息到数据库         │
  │                          ├─ Clients.Caller 回显     │
  │                          ├─ Clients.Client(B) 推送  │
  │<── ReceivePrivateMessage ─┤── ReceivePrivateMessage >│
  │                          │                          │
```

## 数据库表

| 表名 | 说明 |
|------|------|
| `Users` | 用户表（用户名、密码哈希） |
| `ChatMessages` | 消息表（内容、发送者、接收者、会话ID、时间） |
| `PrivateConversations` | 私聊会话表（用户1、用户2、创建时间） |
| `FriendShips` | 好友关系表（请求者、接收者、状态） |
