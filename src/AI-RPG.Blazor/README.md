# AI-RPG.Blazor

AI-RPG 游戏的 Blazor WebAssembly 前端应用。

## 技术栈

- **框架**: Blazor WebAssembly (.NET 10)
- **UI 组件库**: MudBlazor 8.2.0
- **HTTP 客户端**: HttpClient
- **状态管理**: 自定义服务 + 组件状态

## 项目结构

```
AI-RPG.Blazor/
├── wwwroot/                  # 静态资源
│   ├── css/
│   ├── js/
│   ├── index.html           # 入口 HTML
│   └── appsettings.json     # 前端配置
├── Layout/                   # 布局组件
│   ├── MainLayout.razor     # 主布局
│   └── NavMenu.razor        # 导航菜单
├── Pages/                    # 页面
│   ├── Home.razor           # 首页
│   └── Sessions/            # 会话相关页面
│       ├── List.razor       # 会话列表
│       ├── Create.razor     # 创建会话
│       └── Detail.razor     # 会话详情/对话
├── Components/               # 可复用组件
│   └── AddNPCDialog.razor   # 添加 NPC 对话框
├── Models/                   # 数据模型
│   └── DTOs.cs              # DTO 类
├── Services/                 # 服务层
│   ├── SessionService.cs    # 会话服务
│   └── DialogueService.cs   # 对话服务
├── App.razor                 # 根组件
├── Program.cs                # 入口
└── _Imports.razor            # 全局 using
```

## 快速开始

### 1. 配置 API 地址

编辑 `wwwroot/appsettings.json`：

```json
{
  "ApiBaseUrl": "http://localhost:5134"
}
```

### 2. 运行项目

```bash
cd AI-RPG.Blazor
dotnet run
```

应用将在 `http://localhost:5000` 启动。

### 3. 确保 WebAPI 已运行

Blazor 前端需要连接 WebAPI，请确保 AI-RPG.WebAPI 项目已启动。

## 功能说明

### 首页
- 项目介绍
- 快速导航到会话列表或创建会话

### 会话列表
- 查看所有会话
- 创建新会话
- 管理会话状态（开始、暂停、结束）
- 进入会话详情

### 创建会话
- 填写会话标题
- 设置游戏类型和主题
- 描述世界观
- 设置初始场景

### 会话详情/对话
- 左侧：会话信息、参与者列表、场景信息
- 右侧：聊天窗口
  - 显示历史消息
  - 发送新消息
  - AI 自动响应
- 添加 NPC 功能

## 页面路由

| 路由 | 页面 | 说明 |
|------|------|------|
| `/` | 首页 | 项目介绍和导航 |
| `/sessions` | 会话列表 | 查看和管理会话 |
| `/sessions/create` | 创建会话 | 创建新会话 |
| `/sessions/{id}` | 会话详情 | 进入对话界面 |

## 核心流程

```
1. 访问首页
   ↓
2. 点击"创建会话"或"会话列表"
   ↓
3. 创建会话 → 填写信息 → 创建
   ↓
4. 添加 NPC（可选）
   ↓
5. 点击"开始会话"
   ↓
6. 点击"加入会话"
   ↓
7. 发送消息与 AI 对话
   ↓
8. 结束会话
```

## 配置说明

### appsettings.json

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `ApiBaseUrl` | WebAPI 地址 | `http://localhost:5134` |

### 环境变量

| 变量名 | 说明 |
|--------|------|
| `ASPNETCORE_ENVIRONMENT` | 运行环境 |

## 开发说明

### 添加新页面

1. 在 `Pages` 文件夹创建 `.razor` 文件
2. 使用 `@page "/route"` 定义路由
3. 注入需要的服务
4. 实现页面逻辑

### 添加新服务

1. 在 `Services` 文件夹创建服务类
2. 在 `Program.cs` 中注册服务
3. 在页面中使用 `@inject` 注入

### 使用 MudBlazor 组件

```razor
<MudButton Variant="Variant.Filled" Color="Color.Primary">
    按钮文本
</MudButton>

<MudTextField @bind-Value="_text" Label="输入框" Variant="Variant.Outlined" />

<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">标题</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudText>内容</MudText>
    </MudCardContent>
</MudCard>
```

## 注意事项

1. **CORS**: 确保 WebAPI 允许 Blazor 前端的跨域请求
2. **用户身份**: 当前使用硬编码用户 ID (`user-001`)，后续可添加身份验证
3. **会话数据**: 刷新页面后会话列表需要重新加载
4. **流式响应**: SSE 流式响应功能需要 WebAPI 支持

## 与 WebAPI 的关系

```
Blazor (前端)  ←──HTTP──→  WebAPI (后端)
    │                          │
    │ 1. 创建会话               │
    │ ←───────────────────────→ │
    │ 2. 获取会话列表            │
    │ ←───────────────────────→ │
    │ 3. 发送消息               │
    │ ←───────────────────────→ │
    │ 4. 接收 AI 响应           │
    │ ←───────────────────────→ │
```

## 许可证

MIT License
