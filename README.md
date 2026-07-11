# ChatRoom - 实时聊天室

基于 **ASP.NET Core + SignalR + WPF (Prism MVVM)** 的实时聊天应用，支持：
- 一对一私聊
- 好友申请流程（添加/同意/拒绝）
- 群聊（创建 / 成员管理 / 实时消息推送）

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
│   │   └── GroupController.cs                # 群聊 API
│   ├── Hubs/
│   │   └── ChatHub.cs                        # SignalR 消息中心
│   │       ├── RegisterUser                  # 注册连接
│   │       ├── SendPrivateMessage            # 私聊消息
│   │       ├── SendMessage                   # 公共消息
│   │       └── SendGroupMessage              # 群聊消息
│   ├── Models/                               # EF Core 实体
│   │   ├── User.cs
│   │   ├── ChatMessage.cs                    # 消息（含 ConversationId / GroupId）
│   │   ├── PrivateConversation.cs            # 私聊会话
│   │   ├── Group.cs                          # 群聊
│   │   ├── GroupMember.cs                    # 群成员（(GroupId,UserId) 唯一索引）
│   │   └── FriendShip.cs                     # 好友关系（pending/accepted/rejected）
│   ├── Dto/
│   │   ├── UserDto.cs / FriendDto.cs
│   │   ├── ApiResult.cs / LoginResultData.cs
│   │   ├── MessageDto.cs                     # 私聊消息 DTO
│   │   ├── ConversationDto.cs                # 私聊会话 DTO
│   │   ├── FriendRequestDto.cs
│   │   ├── AcceptFriendRequestResultDto.cs
│   │   ├── FriendItemDto.cs
│   │   ├── CreateGroupDto.cs                 # 创建群请求
│   │   └── GroupDto.cs                       # 群聊 DTO
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IFriendService.cs
│   │   ├── IGroupService.cs                  # 群聊服务接口
│   │   └── IUserConnectionManager.cs         # SignalR 连接管理
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── FriendService.cs                  # 好友/会话业务逻辑
│   │   ├── GroupService.cs                   # 群聊业务（必须为好友才能拉入群）
│   │   └── UserConnectionManager.cs          # ConcurrentDictionary<int, string>
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Migrations/                           # EF Core 迁移（含 AddGroupChat / AddGroupMessageSupport）
│   ├── Program.cs                            # 注册 IGroupService 等
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
    │   ├── MainViewModel.cs                  # 主界面协调器（订阅 GroupMessageReceived / GroupCreated）
    │   ├── ChatViewModel.cs
    │   ├── AddFirendDialogViewModel.cs
    │   ├── FriendPanelViewModel.cs           # 好友面板（好友 + 申请）
    │   ├── GroupViewModel.cs                 # 群聊面板（含 LoadGroupsAsync 启动加载）
    │   ├── GroupSessionViewModel.cs          # 单个群聊会话（注入 IChatService 发送群消息）
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
    │   ├── CreateGroupDto.cs
    │   └── GroupDto.cs
    ├── Services/
    │   ├── ChatService.cs                    # SignalR 连接 + 转发聊天/会话/群聊事件
    │   ├── HubCallbackService.cs             # SignalR 回调集中注册
    │   ├── FriendService.cs                  # 好友 HTTP 服务
    │   ├── GroupService.cs                   # 群聊 HTTP 服务
    │   └── NavigationService.cs
    ├── Interfaces/
    │   ├── IChatService.cs                   # 含 GroupMessageReceived / GroupCreated / SendGroupMessageAsync
    │   ├── IFriendService.cs
    │   ├── IGroupService.cs
    │   ├── IHubCallbackService.cs            # 集中注册所有 Hub 回调
    │   └── INavigationService.cs
    └── App.xaml.cs                           # Prism DI 容器
```

## 核心功能

- ✅ 用户注册/登录（HTTP API + 密码哈希）
- ✅ 一对一实时私聊（SignalR）
- ✅ 好友申请流程：发起 → 待处理 → 同意/拒绝 → 自动建会话
- ✅ 好友面板（收到 / 发出的申请 + 好友列表）
- ✅ 群聊全流程：
  - 创建群聊（输入群名 + 多选好友，**只能拉好友进群**）
  - 启动时自动加载我的群聊
  - 创建后通过 SignalR `GroupCreated` 通知在线群成员
  - 群消息持久化到 MySQL
  - 群消息通过 SignalR `ReceiveGroupMessage` 推送给所有在线群成员
- ✅ 抽象的 `SessionViewModel` 基类
- ✅ 职责分离的 `UserConnectionManager` / `IHubCallbackService`
- ✅ 收到消息时自动刷新会话列表

## 群聊流程

### 创建群聊

```
用户A (群主)                              服务端                       用户B/C（在线成员）
  │                                        │                              │
  ├─ 打开 CreateGroupDialog ──────────────>│                              │
  │   （传入 FriendCollection）            │                              │
  ├─ 输入群名 + 勾选成员 (B、C)            │                              │
  ├─ Confirm ─────────────────────────────>│                              │
  │   POST /api/groups/create              │                              │
  │   { OwnerId, GroupName, MemberIds }    │                              │
  │   ─────────────────────────────────> │                              │
  │                                        ├─ 校验：成员必须是 Owner 好友 │
  │                                        ├─ 写入 Group + GroupMembers   │
  │                                        ├─ 查在线成员 B、C             │
  │                                        ├─ SignalR GroupCreated → B/C │
  │<── GroupDto ──────────────────────────┤── ReceiveGroupCreated ──────>│
  │                                        │                              │
  ├─ AddOrUpdateGroup(本地) ─────────────>│                              │
  │   GroupSessionCollection.Add(...)      │                              │
  │                                        │                              │
  │                                        │◄── (B/C 收到后调用)          │
  │                                        │   GET /api/groups/my/{id}    │
```

### 发送群消息

```
用户A                          服务端                       用户B/C（群成员）
  │                              │                              │
  ├─ Send (在 GroupSession) ──>│                              │
  │  ChatService                │                              │
  │   .SendGroupMessageAsync ──>│                              │
  │   InvokeAsync("SendGroupMessage", ...)                    │
  │                              ├─ 校验：是否群成员            │
  │                              ├─ 保存到 ChatMessages         │
  │                              ├─ 查所有群成员 connectionIds │
  │                              ├─ SignalR ReceiveGroupMessage│
  │<── (本地未直接收到，依赖广播)──┤── ReceiveGroupMessage ──────>│
  │                              │                              │
  ├─ GroupPanel.OnMessage ──>   │                              │
  │   MessageCollection.Add(...) │                              │
  │                              │                              │
  ├─ 回到群列表看到 LastMessage  │                              │
```

> ⚠️ 当前 `SendGroupMessage` 服务端**只向在线群成员广播**，离线成员登录后尚未自动拉取群历史消息。如需可后续扩展 `GET /api/groups/{id}/messages` 或在 `RegisterUser` 阶段加载。

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
| `GroupViewModel` | 群聊会话集合 + 创建群 + 接收群消息 + 启动加载 |
| `MainViewModel` | 私聊会话集合 + 协调子面板 + SignalR 连接 |

### 3. SignalR 回调统一注册（`IHubCallbackService`）

把 `_hubConnection.On(...)` 全部抽到 `HubCallbackService`：
- 各 ViewModel 直接订阅回调事件
- `ChatService` 只负责连接 / 发送
- 便于测试与扩展

### 4. 连接管理（`IUserConnectionManager`）

服务端把 `ConcurrentDictionary<int, string> _connections` 从 `ChatHub` 抽出：
- `ChatHub` / `FriendsController` / `GroupController` 共用同一连接字典
- 单元测试更友好

### 5. 群聊安全：只允许拉好友入群

`GroupService.CreateGroupAsync` 校验：
- 群主 / 群成员用户必须存在
- 所有被拉入的成员必须**已与群主是好友**（`FriendShips.Status == "accepted"`）
- 群成员数量 ≥ 1

## API 接口

### HTTP

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
| `POST` | `/api/groups/create` | 创建群聊（仅好友可入群） |
| `GET` | `/api/groups/my/{userId}` | 我加入的群聊 |

### SignalR Hub 方法（`/chathub`）

| 方法 | 说明 |
|------|------|
| `RegisterUser(userId)` | 注册连接，返回会话列表 |
| `SendPrivateMessage(senderId, receiverId, senderName, message)` | 私聊消息 |
| `SendMessage(userId, userName, message)` | 公共消息（保留） |
| `SendGroupMessage(groupId, senderId, userName, message)` | 群消息（非成员抛 HubException） |

### SignalR 客户端事件

| 事件 | 说明 |
|------|------|
| `LoadConversations` | 会话列表刷新 |
| `ReceivePrivateMessage` | 私聊消息 |
| `ReceiveMessage` | 公共消息（保留） |
| `FriendRequestReceived` | 收到好友申请 |
| `FriendRequestStatusChanged` | 好友申请状态变化 |
| `ReceiveGroupMessage` | 群消息推送（含 groupId + ChatMessage） |
| `GroupCreated` | 被加入新群通知（在线群成员） |

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

迁移包含：
- `Init` / `AddUserName` / `MakeSenderIdNullable` / `AddPrivateChatSupport` / `AddPrivateConversation`
- `AddGroupChat`（新增 Groups / GroupMembers 表）
- `AddGroupMessageSupport`（ChatMessage 新增 GroupId）

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
| `ChatMessages` | 消息（SenderId、ReceivedId、ConversationId、**GroupId**、SendTime） |
| `PrivateConversations` | 私聊会话 |
| `Groups` | 群聊（GroupId、GroupName、OwnerId、CreateTime） |
| `GroupMembers` | 群成员（GroupId、UserId、JoinTime，**唯一索引**） |
| `FriendShips` | 好友关系（Status: pending/accepted/rejected） |

## TODO

- [ ] `GroupSessionViewModel` 改为继承 `SessionViewModel`（统一抽象）
- [ ] 离线群消息拉取：注册时加载我的群历史消息 / `GET /api/groups/{id}/messages`
- [ ] 群消息未读计数 + 群消息推送时自动选中会话
- [ ] 清理 `WeatherForecastController.cs` / `WeatherForecast.cs` 模板文件
- [ ] `ChatHub.SendMessage` 公共消息接口是否保留待定
