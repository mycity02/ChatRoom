# ChatRoom

ChatRoom 是一个基于 **ASP.NET Core + SignalR + EF Core + MySQL + WPF Prism MVVM** 的聊天练习项目。

当前项目重点已经从“直接添加好友”调整为“好友申请流程”：用户发送申请，对方同意或拒绝，同意后自动创建私聊会话并发送第一条招呼消息。客户端支持会话列表、好友申请列表、好友列表，以及点击好友打开私聊会话。

## 技术栈

| 层级 | 技术 |
| --- | --- |
| 服务端 | ASP.NET Core (.NET 10) |
| 实时通信 | SignalR |
| 数据访问 | EF Core + Pomelo.EntityFrameworkCore.MySql |
| 数据库 | MySQL |
| 客户端 | WPF (.NET 10) + Prism MVVM |
| HTTP 客户端 | HttpClient + System.Net.Http.Json |
| 认证 | HTTP API + PasswordHasher |

## 当前功能

- 用户注册、登录。
- 登录后进入主聊天窗口。
- SignalR 初始化连接：`ConnectAsync()` -> `RegisterAsync(CurrentUserId)` -> `ChatHub.RegisterUser(userId)`。
- 一对一私聊：发送消息、保存 `ChatMessage`、实时推送 `ReceivePrivateMessage`。
- 登录后加载私聊会话列表：服务端推送 `LoadConversations`，客户端更新 `PrivateSessionCollection`。
- 好友申请：发送申请、收到申请、同意申请、拒绝申请。
- 好友申请实时通知：`FriendRequestReceived`、`FriendRequestStatusChanged`。
- 同意好友申请后：
  - `FriendShip.Status` 改为 `accepted`。
  - 创建或复用 `PrivateConversation`。
  - 创建第一条招呼消息。
  - 通知申请人刷新申请状态并收到招呼消息。
  - 被申请人拿到 `ConversationDto` 后创建并选中私聊会话。
- 好友列表：从数据库查询 `accepted` 好友并显示在好友 Tab。
- 点击好友列表中的好友：创建或选中前端 `PrivateSession`，并切回“会话”Tab。
- SignalR 回调已从 `ChatService` 拆到 `HubCallbackService`，减少业务服务耦合。
- 服务端连接管理已抽象为 `IUserConnectionManager` / `UserConnectionManager`。

## 暂未完成

- 群聊创建。
- 群成员管理。
- 群聊消息发送和接收。
- 私聊历史消息按会话加载。
- 未读消息数量、消息已读状态。
- 更完整的错误提示和 UI 细节。

## 项目结构

```text
ChatRoom/
├─ ChatRoom.Server/
│  ├─ Controllers/
│  │  ├─ AuthController.cs          # 登录、注册 API
│  │  └─ FriendsController.cs       # 好友申请、好友列表、同意/拒绝申请
│  ├─ Hubs/
│  │  └─ ChatHub.cs                 # SignalR Hub，负责注册连接、私聊发送、会话加载
│  ├─ Data/
│  │  └─ AppDbContext.cs            # EF Core DbContext
│  ├─ Models/
│  │  ├─ User.cs
│  │  ├─ ChatMessage.cs
│  │  ├─ PrivateConversation.cs
│  │  └─ FriendShip.cs
│  ├─ Dto/
│  │  ├─ UserDto.cs
│  │  ├─ FriendDto.cs
│  │  ├─ FriendRequestDto.cs
│  │  ├─ FriendItemDto.cs
│  │  ├─ ConversationDto.cs
│  │  ├─ MessageDto.cs
│  │  ├─ ApiResult.cs
│  │  ├─ LoginResultData.cs
│  │  └─ AcceptFriendRequestResultDto.cs
│  ├─ Interfaces/
│  │  ├─ IAuthService.cs
│  │  ├─ IFriendService.cs
│  │  └─ IUserConnectionManager.cs
│  ├─ Services/
│  │  ├─ AuthService.cs
│  │  ├─ FriendService.cs
│  │  └─ UserConnectionManager.cs
│  ├─ Migrations/
│  ├─ Program.cs
│  └─ appsettings.json
│
└─ ChatRoom.Client/
   ├─ Views/
   │  ├─ LoginView.xaml
   │  ├─ MainView.xaml              # 主界面：会话 Tab、好友 Tab、聊天区域
   │  ├─ ChatView.xaml
   │  └─ AddFriendDialog.xaml
   ├─ ViewModels/
   │  ├─ LoginViewModel.cs
   │  ├─ MainViewModel.cs           # 会话列表、当前会话、聊天消息接收
   │  ├─ FriendPanelViewModel.cs    # 好友申请、好友列表、点击好友事件
   │  ├─ ChatViewModel.cs
   │  └─ AddFirendDialogViewModel.cs
   ├─ Models/
   │  ├─ Session.cs
   │  ├─ PrivateSession.cs
   │  ├─ ChatMessage.cs
   │  └─ FriendItem.cs
   ├─ Dto/
   │  ├─ FriendDto.cs
   │  ├─ FriendRequestDto.cs
   │  └─ ConversationDto.cs
   ├─ Interfaces/
   │  ├─ IChatService.cs
   │  ├─ IFriendService.cs
   │  ├─ IHubCallbackService.cs
   │  └─ INavigationService.cs
   ├─ Services/
   │  ├─ ChatService.cs             # SignalR 连接、注册、发送消息
   │  ├─ HubCallbackService.cs      # SignalR 服务端回调统一注册和分发
   │  ├─ FriendService.cs           # 好友相关 HTTP 调用
   │  └─ NavigationService.cs
   └─ App.xaml.cs                   # Prism DI 注册
```

## 核心流程

### 登录和连接

```text
LoginViewModel
-> AuthController.Login
-> NavigationService.NavigateToMainView
-> MainViewModel.InitializeAsync
-> ChatService.ConnectAsync
-> ChatService.RegisterAsync(CurrentUserId)
-> ChatHub.RegisterUser(userId)
-> UserConnectionManager.AddConnection(userId, connectionId)
-> ChatHub.LoadConversation(userId)
-> HubCallbackService 收到 LoadConversations
-> MainViewModel.OnConversationLoad
```

### 私聊发送

```text
PrivateSession.SendCommand
-> PrivateSession.SendAsync
-> ChatService.SendPrivateMessageAsync
-> ChatHub.SendPrivateMessage
-> 获取或创建 PrivateConversation
-> 保存 ChatMessage
-> Clients.Caller.ReceivePrivateMessage
-> 在线接收者 ReceivePrivateMessage
-> 双方 LoadConversation 刷新会话列表
```

### 好友申请

```text
申请人点击添加好友
-> FriendPanelViewModel.AddFriendRequest
-> FriendService.AddFriendRequestAsync
-> POST /api/friends/request
-> FriendService.AddFriendRequestAsync(server)
-> 写入 FriendShip(status = pending)
-> 如果被申请人在线，推送 FriendRequestReceived
-> 申请人本地加入 SentFriendRequestCollection
```

### 同意好友申请

```text
被申请人点击同意
-> FriendPanelViewModel.AcceptFriendRequest
-> FriendService.AcceptFriendRequestAsync
-> POST /api/friends/requests/{friendshipId}/accept
-> Server FriendService.AcceptAddFriendRequestAsync
-> FriendShip.Status = accepted
-> 创建或复用 PrivateConversation
-> 创建第一条招呼消息
-> 推送 FriendRequestStatusChanged 给申请人
-> 推送 ReceivePrivateMessage 给申请人
-> HTTP 返回 ConversationDto 给被申请人
-> FriendPanelViewModel.FriendRequestAccepted
-> MainViewModel.OnFriendRequestAccepted
-> 创建并选中 PrivateSession
```

### 点击好友打开聊天

```text
好友 Tab 点击某个 FriendItem
-> MainView.xaml SelectedItem 绑定到 FriendPanel.SelectedFriend
-> FriendPanelViewModel.SelectedFriendChanged
-> MainViewModel.OnFriendSelected
-> 从 PrivateSessionCollection 查找会话
-> 不存在则创建新的 PrivateSession
-> SelectedSession = session
-> SelectedLeftTabIndex = 0，切回“会话”Tab
```

## HTTP API

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| POST | `/api/auth/login` | 登录 |
| POST | `/api/auth/register` | 注册 |
| POST | `/api/friends/add` | 直接添加好友，旧接口保留 |
| POST | `/api/friends/request` | 发送好友申请 |
| GET | `/api/friends/requests/received/{userId}` | 获取收到的好友申请 |
| GET | `/api/friends/requests/sent/{userId}` | 获取发出的好友申请 |
| POST | `/api/friends/requests/{friendshipId}/accept` | 同意好友申请 |
| POST | `/api/friends/requests/{friendshipId}/reject` | 拒绝好友申请 |
| GET | `/api/friends/list/{userId}` | 获取好友列表 |

## SignalR

Hub 地址：

```text
/chathub
```

### 客户端调用服务端

| 方法 | 说明 |
| --- | --- |
| `RegisterUser(userId)` | 注册当前用户连接，并加载会话列表 |
| `SendPrivateMessage(senderId, receiverId, senderName, message)` | 发送私聊消息 |
| `SendMessage(userId, userName, message)` | 旧群聊广播方法，当前不是主线功能 |

### 服务端推送客户端

| 事件 | 说明 |
| --- | --- |
| `LoadConversations` | 推送当前用户的私聊会话列表 |
| `ReceivePrivateMessage` | 推送私聊消息 |
| `ReceiveMessage` | 旧群聊广播消息 |
| `LoadHistory` | 旧群聊历史消息，目前未启用 |
| `FriendRequestReceived` | 被申请人收到新的好友申请 |
| `FriendRequestStatusChanged` | 申请人收到申请状态变化 accepted/rejected |

## 数据表

| 表 | 说明 |
| --- | --- |
| `Users` | 用户 |
| `FriendShips` | 好友关系和申请状态，`Status` 包含 `pending`、`accepted`、`rejected` |
| `PrivateConversations` | 私聊会话 |
| `ChatMessages` | 聊天消息，包含私聊消息和旧群聊消息字段 |

## 运行方式

### 1. 配置数据库连接

编辑 `ChatRoom.Server/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "Default": "server=localhost;port=3306;database=ChatRoom;user=root;password=你的密码"
  }
}
```

### 2. 创建数据库

```sql
CREATE DATABASE ChatRoom CHARACTER SET utf8mb4;
```

### 3. 执行迁移

```bash
dotnet ef database update --project ChatRoom.Server
```

### 4. 启动服务端

```bash
dotnet run --project ChatRoom.Server
```

默认客户端代码使用：

```text
http://localhost:5000
```

### 5. 启动 WPF 客户端

```bash
dotnet run --project ChatRoom.Client
```

## 当前设计说明

- `ChatService` 只负责 SignalR 连接、注册和发送消息。
- `HubCallbackService` 统一注册 `_hubConnection.On(...)`，再通过事件分发给 ViewModel。
- `FriendPanelViewModel` 负责好友申请、好友列表和好友选择事件。
- `MainViewModel` 负责主聊天窗口状态，包括 `PrivateSessionCollection`、`SelectedSession` 和左侧 Tab 切换。
- 好友列表和会话列表不是同一个概念：好友列表来自 `FriendShips`，会话列表来自 `PrivateConversations`。
- 点击好友时，如果会话列表里没有对应 `PrivateSession`，客户端会临时创建一个前端会话；真正的数据库会话会在发送消息或同意申请时由服务端创建或复用。

## 后续计划

1. 实现私聊历史消息加载。
2. 完善好友同意/拒绝后的好友列表实时刷新。
3. 实现创建群聊：选择好友、创建群、保存群成员。
4. 实现群聊消息：加入 SignalR Group、发送群消息、接收群消息。
5. 优化 WPF UI：消息气泡、自动滚动、未读数量、右键菜单。

