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
| MVVM | Prism (BindableBase + DelegateCommand + Dialog + DI) |
| HTTP 客户端 | HttpClient + PostAsJsonAsync |

## 项目结构

```
ChatRoom/
├── ChatRoom.Server/                       # 服务端
│   ├── Controllers/
│   │   ├── AuthController.cs              # 登录/注册 API
│   │   └── FriendsController.cs           # 好友/会话 API（添加好友）
│   ├── Hubs/
│   │   └── ChatHub.cs                     # SignalR 消息中心（私聊 + 会话管理）
│   ├── Models/
│   │   ├── User.cs                        # 用户实体
│   │   ├── ChatMessage.cs                 # 消息实体
│   │   ├── PrivateConversation.cs         # 私聊会话实体
│   │   ├── FriendShip.cs                  # 好友关系实体
│   │   └── Conversation.cs                # 会话返回 DTO（服务端）
│   ├── Dto/
│   │   ├── UserDto.cs                     # 用户请求参数
│   │   ├── FriendDto.cs                   # 添加好友请求参数
│   │   ├── MessageDto.cs                  # 私聊消息 DTO
│   │   ├── ApiResult.cs                   # 统一响应格式
│   │   └── LoginResultData.cs             # 登录响应数据
│   ├── Interfaces/
│   │   ├── IAuthService.cs                # 认证服务接口
│   │   └── IFriendService.cs              # 好友服务接口
│   ├── Services/
│   │   ├── AuthService.cs                 # 认证服务实现
│   │   └── FriendService.cs               # 好友/会话服务实现
│   ├── Data/
│   │   └── AppDbContext.cs                # EF Core 数据库上下文
│   ├── Migrations/                        # 数据库迁移文件
│   ├── Program.cs                         # 启动入口（注册服务、路由、Hub）
│   └── appsettings.json                   # 配置文件（连接字符串）
│
└── ChatRoom.Client/                       # WPF 客户端
    ├── Views/
    │   ├── LoginView.xaml                 # 登录/注册界面
    │   ├── MainView.xaml                  # 主界面（会话列表 + 聊天区）
    │   ├── ChatView.xaml                  # 单聊界面
    │   └── AddFriendDialog.xaml           # 添加好友弹窗
    ├── ViewModels/
    │   ├── LoginViewModel.cs              # 登录/注册逻辑
    │   ├── MainViewModel.cs               # 主界面逻辑（会话管理 + 消息接收）
    │   ├── ChatViewModel.cs               # 聊天逻辑
    │   └── AddFirendDialogViewModel.cs    # 添加好友弹窗逻辑
    ├── Models/
    │   ├── ChatMessage.cs                 # 消息模型
    │   ├── Session.cs                     # 会话抽象基类
    │   ├── PrivateSession.cs              # 私聊会话
    │   └── Conversation.cs                # 会话列表模型
    ├── Dto/
    │   └── FriendDto.cs                   # 添加好友请求 DTO（客户端）
    ├── Services/
    │   ├── ChatService.cs                 # SignalR 通信服务
    │   ├── FriendService.cs               # 好友 HTTP 服务
    │   └── NavigationService.cs           # 页面导航服务
    ├── Interfaces/
    │   ├── IChatService.cs                # 通信接口
    │   ├── IFriendService.cs              # 好友服务接口
    │   └── INavigationService.cs          # 导航接口
    └── App.xaml.cs                        # Prism 启动配置（DI 注册）
```

## 功能

- ✅ 用户注册/登录（HTTP API + 密码哈希）
- ✅ 一对一实时私聊（SignalR 双工通信）
- ✅ 好友系统（按用户名添加好友，自动创建会话）
- ✅ 自动欢迎语（首次添加好友时双方收到欢迎消息）
- ✅ 会话列表（自动加载所有私聊会话 + 最后一条消息预览）
- ✅ 会话切换（保持选中状态、移除已失效会话）
- ✅ 消息持久化到 MySQL（含发送者/接收者/会话关联）
- ✅ 消息实时推送（双方同时收到，回显给自己）
- ✅ 在线连接管理（SignalR UserConnection 字典）

## 一对一会话核心流程

### 1. 添加好友并创建会话

```
客户端                              服务端
  │                                  │
  ├─ POST /api/friends/add ─────────>│
  │   { currentUserId, friendName }   │
  │                                  ├─ 查找好友用户
  │                                  ├─ 检查/创建 FriendShip
  │                                  ├─ 检查/创建 PrivateConversation
  │                                  ├─ 首次添加：插入欢迎消息
  │<── 200 OK (Conversation) ────────┤
  │                                  │
  ├─ 客户端跳转到该会话 ──────────────>│
```

### 2. 一对一聊天

```
客户端A                     服务端                      客户端B
  │                          │                          │
  ├─ RegisterUser(userId) ──>│                          │
  │<── LoadConversations ────│  （返回会话列表）          │
  │                          │                          │
  ├─ SendPrivateMessage() ──>│                          │
  │                          ├─ GetOrCreateConversation  │
  │                          ├─ 保存消息到数据库         │
  │                          ├─ 推送给自己 (Caller)      │
  │                          ├─ 推送给对方（如在线）      │
  │                          ├─ 刷新双方会话列表         │
  │<── ReceivePrivateMessage ─┤── ReceivePrivateMessage >│
  │                          │                          │
```

## 关键设计

### 抽象 `Session` 基类

- `Session` 抽象类封装通用会话属性（`OtherUserName`、`LastMessage`、`MessageCollection`、`NewMessage`、`SendCommand`）
- `PrivateSession` 继承并实现 `SendAsync()`，调用 `IChatService.SendPrivateMessageAsync`
- 便于未来扩展 `GroupSession` 等类型

### 消息流推送策略

- 发送方立即收到自己发的消息（`Clients.Caller`）
- 接收方如在线则收到（`Clients.Client(connectionId)`），并刷新会话列表
- 双方在收到消息后都会重新拉取会话列表，保持最新顺序

## API 接口

| 方法 | 路径 | 说明 |
|------|------|------|
| `POST` | `/api/auth/login` | 用户登录 |
| `POST` | `/api/auth/register` | 用户注册 |
| `POST` | `/api/friends/add` | 添加好友并创建会话 |

### SignalR Hub 方法

| 方法 | 说明 |
|------|------|
| `RegisterUser(userId)` | 用户连接时注册，返回会话列表 |
| `SendPrivateMessage(senderId, receiverId, senderName, message)` | 发送私聊消息 |
| `SendMessage(userId, userName, message)` | 群聊消息（保留接口） |

### SignalR 客户端事件

| 事件 | 说明 |
|------|------|
| `LoadConversations` | 服务端推送会话列表 |
| `ReceivePrivateMessage` | 服务端推送私聊消息 |
| `ReceiveMessage` | 服务端推送群聊消息 |

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

## 数据库表

| 表名 | 说明 |
|------|------|
| `Users` | 用户表（用户名、密码哈希） |
| `ChatMessages` | 消息表（内容、发送者、接收者、会话ID、时间） |
| `PrivateConversations` | 私聊会话表（用户1、用户2、创建时间） |
| `FriendShips` | 好友关系表（请求者、接收者、状态） |
