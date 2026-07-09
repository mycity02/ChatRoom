# ChatRoom - 实时聊天室

基于 **ASP.NET Core + SignalR + WPF (Prism MVVM)** 的实时聊天应用，支持：
- 一对一私聊
- 好友申请流程
- 群聊（创建/UI/消息接口已搭建，服务端 API 暂未实现）

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
| HTTP 客户端 | HttpClient + PostAsJsonAsync / GetFromJsonAsync |

## 项目结构

```
ChatRoom/
├── ChatRoom.Server/                          # 服务端
│   ├── Controllers/
│   │   ├── AuthController.cs                 # 登录/注册 API
│   │   ├── FriendsController.cs              # 好友/会话 API
│   │   └── WeatherForecastController.cs      # 模板示例（可删）
│   ├── Hubs/
│   │   └── ChatHub.cs                        # SignalR 消息中心（私聊 + 群聊保留）
│   ├── Models/                               # EF Core 实体
│   │   ├── User.cs
│   │   ├── ChatMessage.cs                    # 私聊消息
│   │   ├── PrivateConversation.cs            # 私聊会话
│   │   └── FriendShip.cs                     # 好友关系（pending/accepted/rejected）
│   ├── Dto/
│   │   ├── UserDto.cs / FriendDto.cs
│   │   ├── ApiResult.cs / LoginResultData.cs
│   │   ├── MessageDto.cs                     # 私聊消息 DTO
│   │   ├── ConversationDto.cs                # 私聊会话 DTO
│   │   ├── FriendRequestDto.cs
│   │   ├── AcceptFriendRequestResultDto.cs
│   │   └── FriendItemDto.cs
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IFriendService.cs
│   │   └── IUserConnectionManager.cs         # SignalR 连接管理
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── FriendService.cs                  # 好友/会话业务逻辑
│   │   └── UserConnectionManager.cs          # ConcurrentDictionary<int, string>
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Migrations/                           # EF Core 迁移
│   ├── WeatherForecast.cs                    # 模板示例（可删）
│   ├── Program.cs
│   └── appsettings.json
│
└── ChatRoom.Client/                          # WPF 客户端
    ├── Views/
    │   ├── LoginView.xaml                    # 登录/注册
    │   ├── MainView.xaml                     # 主界面（好友/会话/群聊 Tab）
    │   ├── ChatView.xaml
    │   ├── AddFriendDialog.xaml              # 添加好友弹窗
    │   └── CreateGroupDialog.xaml            # 创建群聊弹窗
    ├── ViewModels/
    │   ├── LoginViewModel.cs
    │   ├── MainViewModel.cs                  # 主界面协调器
    │   ├── ChatViewModel.cs
    │   ├── AddFirendDialogViewModel.cs
    │   ├── FriendPanelViewModel.cs           # 好友面板（好友 + 申请）
    │   ├── GroupViewModel.cs                 # 群聊面板
    │   ├── GroupSessionViewModel.cs          # 单个群聊会话
    │   ├── CreateGroupDialogViewModel.cs     # 创建群弹窗逻辑
    │   ├── CreateGroupMemberItemViewModel.cs # 群成员勾选项
    │   ├── SessionViewModel.cs               # 会话抽象基类（BindableBase）
    │   └── PrivateSessionViewModel.cs        # 私聊会话
    ├── Models/
    │   ├── ChatMessage.cs
    │   └── FriendItem.cs
    ├── Dto/
    │   ├── FriendDto.cs / FriendRequestDto.cs
    │   ├── ConversationDto.cs
    │   ├── CreateGroupDto.cs                 # 创建群请求
    │   └── GroupDto.cs                       # 群聊 DTO
    ├── Services/
    │   ├── ChatService.cs                    # SignalR 连接与发送
    │   ├── HubCallbackService.cs             # SignalR 回调注册
    │   ├── FriendService.cs                  # 好友 HTTP 服务
    │   ├── GroupService.cs                   # 群聊 HTTP 服务
    │   └── NavigationService.cs
    ├── Interfaces/
    │   ├── IChatService.cs
    │   ├── IFriendService.cs
    │   ├── IGroupService.cs                  # 群聊服务接口
    │   ├── IHubCallbackService.cs
    │   └── INavigationService.cs
    └── App.xaml.cs                           # Prism DI 容器
```

## 核心功能

- ✅ 用户注册/登录（HTTP API + 密码哈希）
- ✅ 一对一实时私聊（SignalR）
- ✅ 好友申请流程：发起 → 待处理 → 同意/拒绝 → 自动建会话
- ✅ 好友面板（收到 / 发出的申请 + 好友列表）
- ✅ 群聊 UI（创建群弹窗 + 群聊 Tab + `GroupSessionViewModel`）
- 🚧 群聊消息：客户端已搭好 `MessageCollection` / `NewMessage` / `SendCommand`，**服务端 API 待实现**
- ✅ 抽象的 `SessionViewModel` 基类（便于扩展会话类型）
- ✅ 职责分离的 `UserConnectionManager` / `IHubCallbackService`
- ✅ 消息持久化到 MySQL
- ✅ 收到消息时自动刷新会话列表

## 群聊流程

### 创建群聊（客户端已实现，服务端待实现）

```
用户                                  服务端
  │                                    │
  ├─ 打开 CreateGroupDialog ──────────>│
  │   （传入 FriendCollection）        │
  ├─ 输入群名 + 勾选成员               │
  ├─ Confirm ────────────────────────>│
  │                                    │
  │  POST /api/groups/create           │
  │  { OwnerId, GroupName, MemberIds } │
  │ ─────────────────────────────────> │
  │                                    ├─ 🚧 GroupsController 待实现
  │<── GroupDto ─────────────────────┤
  │                                    │
  ├─ AddOrUpdateGroup ───────────────>│
  │   （更新 GroupSessionCollection）  │
  │   （自动选中新建的群）             │
```

### 群聊消息（占位，**待实现**）

> 当前 `GroupSessionViewModel.SendAsync()` 已搭好框架（`MessageCollection`、`NewMessage`、`SendCommand`），
> 但 `IGroupService.CreateGroupAsync` / `GetMyGroupAsync` 调用的是**未实现**的服务端 API。
> 后续需要：
> - 服务端：`GroupsController`（`POST /api/groups/create`、`GET /api/groups/my/{userId}`）
> - 服务端：`IGroupService` / `GroupService`（创建群、查我的群、群成员入库）
> - 服务端：EF Core 实体 `Group.cs` / `GroupMember.cs`
> - SignalR：`ChatHub.SendGroupMessage` 广播 + `ReceiveGroupMessage` 推送

## 关键设计

### 1. Session → SessionViewModel 重构

会话从 Models 层上移到 ViewModels 层，所有属性继承自 `BindableBase`：

```
SessionViewModel (abstract, BindableBase)
└── PrivateSessionViewModel  (OtherUserId + 私聊 SendAsync)
```

`GroupSessionViewModel` 暂独立继承 `BindableBase`，因为群聊字段差异较大（`GroupId` vs `OtherUserId`），后续如需统一可调整。

### 2. 面板职责拆分

`MainViewModel` 拆为三个协调单元：

| 子 ViewModel | 职责 |
|--------------|------|
| `FriendPanelViewModel` | 好友 + 好友申请（收到/发出 + 同意/拒绝） |
| `GroupViewModel` | 群聊会话集合 + 创建群 + 接收群消息 |
| `MainViewModel` | 私聊会话集合 + 协调子面板 + SignalR 连接 |

### 3. SignalR 回调统一注册（`IHubCallbackService`）

把 `_hubConnection.On(...)` 全部抽到 `HubCallbackService`：
- 各 ViewModel 直接订阅回调事件
- `ChatService` 只负责连接 / 发送
- 便于测试与扩展

### 4. 连接管理（`IUserConnectionManager`）

服务端把 `ConcurrentDictionary<int, string> _connections` 从 `ChatHub` 抽出：
- `ChatHub` / `FriendsController` 共用同一连接字典
- 单元测试更友好

## API 接口

### HTTP（已实现）

| 方法 | 路径 | 说明 |
|------|------|------|
| `POST` | `/api/auth/login` | 登录 |
| `POST` | `/api/auth/register` | 注册 |
| `POST` | `/api/friends/add` | 直接添加好友（保留） |
| `POST` | `/api/friends/request` | 发送好友申请 |
| `GET` | `/api/friends/requests/received/{userId}` | 收到的好友申请 |
| `GET` | `/api/friends/requests/sent/{userId}` | 发出的好友申请 |
| `POST` | `/api/friends/requests/{friendshipId}/accept` | 同意好友申请 |
| `POST` | `/api/friends/requests/{friendshipId}/reject` | 拒绝好友申请 |
| `GET` | `/api/friends/list/{userId}` | 我的好友列表 |

### HTTP（🚧 客户端调用但服务端未实现）

| 方法 | 路径 | 说明 |
|------|------|------|
| `POST` | `/api/groups/create` | 创建群聊 |
| `GET` | `/api/groups/my/{userId}` | 我加入的群聊 |

### SignalR Hub 方法（`/chathub`）

| 方法 | 说明 |
|------|------|
| `RegisterUser(userId)` | 注册连接，返回会话列表 |
| `SendPrivateMessage(senderId, receiverId, senderName, message)` | 私聊消息 |
| `SendMessage(userId, userName, message)` | 群聊（保留接口） |

### SignalR 客户端事件

| 事件 | 说明 |
|------|------|
| `LoadConversations` | 会话列表刷新 |
| `ReceivePrivateMessage` | 私聊消息 |
| `ReceiveMessage` | 群聊消息（保留） |
| `FriendRequestReceived` | 收到好友申请 |
| `FriendRequestStatusChanged` | 好友申请状态变化 |

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
| `ChatMessages` | 私聊消息（SenderId、ReceivedId、ConversationId、SendTime） |
| `PrivateConversations` | 私聊会话 |
| `FriendShips` | 好友关系（Status: pending/accepted/rejected） |

> 注：群聊相关表（`Groups` / `GroupMembers` / `GroupMessages`）需后续迁移添加。

## TODO

- [ ] 服务端实现 `GroupsController` / `IGroupService` / `GroupService`
- [ ] EF Core 实体：`Group.cs` / `GroupMember.cs` / `GroupMessage.cs` + 迁移
- [ ] `IChatService.SendGroupMessageAsync` + `ChatHub.SendGroupMessage` 广播
- [ ] 客户端 `GroupSessionViewModel.SendAsync` 调用 `IChatService.SendGroupMessageAsync`
- [ ] `GroupSessionViewModel` 改为继承 `SessionViewModel`（统一抽象）
- [ ] 清理 `WeatherForecastController.cs` / `WeatherForecast.cs` 模板文件
