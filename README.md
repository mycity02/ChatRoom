# ChatRoom - 实时聊天室

基于 **ASP.NET Core + SignalR + WPF (Prism MVVM)** 的实时聊天应用。

## 技术栈

| 层 | 技术 |
|------|------|
| 服务端 | ASP.NET Core (.NET 10) + SignalR + EF Core + MySQL |
| 客户端 | WPF (.NET 10) + Prism + SignalR Client |
| 数据库 | MySQL |
| ORM | EF Core (Pomelo.EntityFrameworkCore.MySql) |
| 认证 | HTTP API + ASP.NET Core Identity PasswordHasher |
| 实时通信 | SignalR WebSocket 自动降级 |
| MVVM | Prism (BindableBase + DelegateCommand + DI) |

## 项目结构

```
ChatRoom/
├── ChatRoom.Server/              # 服务端
│   ├── Controllers/
│   │   └── AuthController.cs     # 登录/注册 API
│   ├── Hubs/
│   │   └── ChatHub.cs           # SignalR 消息中心
│   ├── Models/
│   │   ├── User.cs              # 用户实体
│   │   └── ChatMessage.cs       # 消息实体
│   ├── Dto/
│   │   ├── UserDto.cs           # 请求参数
│   │   ├── ApiResult.cs         # 统一响应格式
│   │   └── LoginResultData.cs   # 登录响应数据
│   ├── Interfaces/
│   │   └── IAuthService.cs      # 认证服务接口
│   ├── Services/
│   │   └── AuthService.cs       # 认证服务实现
│   ├── Data/
│   │   └── AppDbContext.cs      # EF Core 数据库上下文
│   ├── Migrations/              # 数据库迁移文件
│   ├── Program.cs               # 启动入口
│   └── appsettings.json         # 配置文件(连接字符串)
│
└── ChatRoom.Client/              # WPF 客户端
    ├── Views/
    │   ├── LoginView.xaml        # 登录/注册界面
    │   └── ChatView.xaml         # 聊天界面
    ├── ViewModels/
    │   ├── LoginViewModel.cs     # 登录逻辑
    │   └── ChatViewModel.cs      # 聊天逻辑
    ├── Models/
    │   └── ChatMessage.cs        # 消息模型
    ├── Services/
    │   ├── ChatService.cs        # SignalR 通信服务
    │   └── NavigationService.cs  # 页面导航服务
    ├── Interfaces/
    │   ├── IChatService.cs       # 通信接口
    │   └── INavigationService.cs # 导航接口
    └── App.xaml.cs               # Prism 启动配置
```

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

## 功能

- 用户注册/登录（HTTP API + 密码哈希）
- 实时群聊（SignalR 广播）
- 历史消息加载（连接时自动推送最近 50 条）
- 消息持久化到 MySQL
- 连接状态指示

## 消息流程

```
客户端A                       服务端                       客户端B
  │                            │                            │
  ├─ InvokeAsync("SendMsg") ──>│                            │
  │                            ├─ ChatHub.SendMessage()     │
  │                            ├─ 存数据库                   │
  │                            ├─ Clients.All.SendAsync()   │
  │                            │                            │
  │<── On("ReceiveMsg") ───────┼──── On("ReceiveMsg") ────>│
  │  UI 显示消息              │          UI 显示消息        │
```
