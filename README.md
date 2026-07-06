# ChatRoom - 实时聊天室

基于 **ASP.NET Core + SignalR + WPF (Prism MVVM)** 的一对一实时聊天应用，支持好友申请流程。

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
├── ChatRoom.Server/                          # 服务端
│   ├── Controllers/
│   │   ├── AuthController.cs                 # 登录/注册 API
│   │   └── FriendsController.cs              # 好友/会话 API
│   │       ├── add           直接添加好友（保留旧接口）
│   │       ├── request       发送好友申请
│   │       ├── accept        同意好友申请
│   │       ├── reject        拒绝好友申请
│   │       ├── received/{id} 我收到的好友申请
│   │       └── sent/{id}     我发出的好友申请
│   ├── Hubs/
│   │   └── ChatHub.cs                        # SignalR 消息中心（私聊 + 会话）
│   ├── Models/                               # EF Core 实体
│   │   ├── User.cs
│   │   ├── ChatMessage.cs
│   │   ├── PrivateConversation.cs            # 私聊会话
│   │   └── FriendShip.cs                     # 好友关系（pending / accepted / rejected）
│   ├── Dto/                                  # 传输对象
│   │   ├── UserDto.cs / FriendDto.cs         # 请求参数
│   │   ├── ApiResult.cs / LoginResultData.cs # 认证响应
│   │   ├── MessageDto.cs                     # 私聊消息 DTO
│   │   ├── ConversationDto.cs                # 会话 DTO
│   │   ├── FriendRequestDto.cs               # 好友申请 DTO
│   │   └── AcceptFriendRequestResultDto.cs   # 同意申请返回结果
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IFriendService.cs
│   │   └── IUserConnectionManager.cs         # 用户连接管理抽象
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── FriendService.cs                  # 好友/会话业务逻辑
│   │   └── UserConnectionManager.cs          # SignalR 连接管理（ConcurrentDictionary）
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Migrations/
│   ├── Program.cs                            # 入口（DI 注册 + Hub 映射）
│   └── appsettings.json
│
└── ChatRoom.Client/                          # WPF 客户端
    ├── Views/
    │   ├── LoginView.xaml                    # 登录/注册
    │   ├── MainView.xaml                     # 主界面（好友面板 + 会话列表 + 聊天区）
    │   ├── ChatView.xaml
    │   └── AddFriendDialog.xaml              # 添加好友弹窗
    ├── ViewModels/
    │   ├── LoginViewModel.cs
    │   ├── MainViewModel.cs                  # 会话列表管理 + 消息接收
    │   ├── ChatViewModel.cs
    │   ├── AddFirendDialogViewModel.cs
    │   └── FriendPanelViewModel.cs           # 好友申请面板（收到/发出 + 同意/拒绝）
    ├── Models/
    │   ├── ChatMessage.cs
    │   ├── Session.cs                        # 会话抽象基类
    │   ├── PrivateSession.cs                 # 私聊会话
    │   └── FriendItem.cs                     # 好友列表项
    ├── Dto/                                  # 传输对象
    │   ├── FriendDto.cs / FriendRequestDto.cs
    │   └── ConversationDto.cs
    ├── Services/
    │   ├── ChatService.cs                    # SignalR 连接与发送
    │   ├── HubCallbackService.cs             # SignalR 回调统一注册（拆分自 ChatService）
    │   ├── FriendService.cs                  # 好友 HTTP 服务
    │   └── NavigationService.cs
    ├── Interfaces/
    │   ├── IChatService.cs
    │   ├── IFriendService.cs
    │   ├── IHubCallbackService.cs
    │   └── INavigationService.cs
    └── App.xaml.cs                           # Prism DI 容器
```

## 核心功能

- ✅ 用户注册/登录（HTTP API + 密码哈希）
- ✅ 一对一实时私聊（SignalR）
- ✅ **好友申请流程**：发起 → 待处理 → 同意/拒绝 → 自动建会话
- ✅ **好友面板**：显示收到 / 发出的申请，实时推送
- ✅ 同意申请后自动发送招呼消息
- ✅ 抽象的 `Session` 基类（便于扩展群聊）
- ✅ 抽象的 `UserConnectionManager` / `IHubCallbackService`（职责分离）
- ✅ 消息持久化到 MySQL
- ✅ 会话列表（按最后消息时间排序）
- ✅ 收到消息时实时刷新会话列表

## 好友申请流程

```
用户A                                  服务端                       用户B
  │                                     │                           │
  ├─ POST /api/friends/request ───────>│                           │
  │                                     ├─ 写入 FriendShip(pending) │
  │                                     ├─ TryGetConnection(B)      │
  │<── 200 FriendRequestDto ────────────┤── FriendRequestReceived ─>│
  │                                     │                           │
  │                                     │<── POST .../accept ───────┤
  │                                     ├─ Status = accepted        │
  │                                     ├─ 创建 PrivateConversation │
  │                                     ├─ 插入招呼消息             │
  │<── FriendRequestStatusChanged ──────┤── ReceivePrivateMessage ─>│
  │<── ReceivePrivateMessage (招呼) ────┤                           │
  │                                     │                           │
  ├─ (UI 上双方出现该会话) ──────────────┤───────────────────────────┤
```

## 关键设计

### 1. 职责分离：`HubCallbackService`

把 SignalR 服务端→客户端的所有回调（`_hubConnection.On(...)`）抽到 `IHubCallbackService` / `HubCallbackService`：
- `ChatService` 只负责连接 + 发送 + 转发聊天/会话事件
- `FriendPanelViewModel` 直接订阅 `FriendRequestReceived` / `FriendRequestStatusChanged`
- 各 ViewModel 解耦，互不依赖

### 2. 抽象：`UserConnectionManager`

服务端把 `ConcurrentDictionary<int, string> _connections` 从 `ChatHub` 抽到 `IUserConnectionManager`：
- `ChatHub` 不再关心连接管理细节
- `FriendsController` 也可注入同一个管理器，用于好友申请实时推送
- 单元测试更友好

### 3. 抽象：`Session` 基类

- `Session` 抽象通用属性（`OtherUserName`、`LastMessage`、`MessageCollection`、`NewMessage`、`SendCommand`）
- `PrivateSession` 实现 `SendAsync()` 调用 `IChatService.SendPrivateMessageAsync`
- 后续可扩展 `GroupSession`

## API 接口

### HTTP

| 方法 | 路径 | 说明 |
|------|------|------|
| `POST` | `/api/auth/login` | 登录 |
| `POST` | `/api/auth/register` | 注册 |
| `POST` | `/api/friends/add` | 直接添加好友（保留旧逻辑） |
| `POST` | `/api/friends/request` | 发送好友申请 |
| `GET` | `/api/friends/requests/received/{userId}` | 收到的好友申请 |
| `GET` | `/api/friends/requests/sent/{userId}` | 发出的好友申请 |
| `POST` | `/api/friends/requests/{friendshipId}/accept` | 同意好友申请 |
| `POST` | `/api/friends/requests/{friendshipId}/reject` | 拒绝好友申请 |

### SignalR Hub 方法（`/chathub`）

| 方法 | 说明 |
|------|------|
| `RegisterUser(userId)` | 注册用户连接，返回会话列表 |
| `SendPrivateMessage(senderId, receiverId, senderName, message)` | 发送私聊消息 |
| `SendMessage(userId, userName, message)` | 群聊（保留） |

### SignalR 客户端事件

| 事件 | 说明 |
|------|------|
| `LoadConversations` | 会话列表刷新 |
| `ReceivePrivateMessage` | 收到私聊消息 |
| `ReceiveMessage` | 群聊消息 |
| `FriendRequestReceived` | 收到新的好友申请 |
| `FriendRequestStatusChanged` | 好友申请状态变化（accepted/rejected） |

## 运行

### 1. 配置数据库

编辑 `ChatRoom.Server/appsettings.json`：

```json
"ConnectionStrings": {
  "Default": "server=你的服务器;port=3306;database=ChatRoom;user=用户名;password=密码"
}
```

### 2. 创建数据库

```sql
CREATE DATABASE ChatRoom CHARACTER SET utf8mb4;
```

### 3. 执行迁移

```bash
cd ChatRoom.Server
dotnet ef database update
```

### 4. 启动服务端

```bash
cd ChatRoom.Server
dotnet run
```

监听 `http://localhost:5000`。

### 5. 启动客户端

```bash
dotnet run --project ChatRoom.Client
```

## 数据库表

| 表名 | 说明 |
|------|------|
| `Users` | 用户 |
| `ChatMessages` | 消息（含 SenderId、ReceivedId、ConversationId、SendTime） |
| `PrivateConversations` | 私聊会话 |
| `FriendShips` | 好友关系（Status: pending/accepted/rejected） |
