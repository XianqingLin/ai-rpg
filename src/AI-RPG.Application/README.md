# AI-RPG.Application

AI-RPG 的应用层，负责编排领域对象完成业务用例。

## 项目定位

位于 **Domain 层** 之上，协调领域对象和 AI 能力完成具体业务场景。

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
├─────────────────────────────────────┤
│      AI-RPG.Application             │  ← 本层
│  - 应用服务、DTO、用例编排           │
├─────────────────────────────────────┤
│         AI-RPG.Domain               │
├─────────────────────────────────────┤
│      AI-RPG.AICapabilities          │
├─────────────────────────────────────┤
│         Infrastructure Layer        │
└─────────────────────────────────────┘
```

## 项目结构

```
AI-RPG.Application/
├── DTOs/                            # 数据传输对象
│   ├── SessionDtos.cs
│   ├── ParticipantDtos.cs
│   └── DialogueDtos.cs
├── Interfaces/                      # 应用服务接口
│   ├── ISessionAppService.cs
│   └── IDialogueAppService.cs
├── Services/                        # 应用服务实现
│   ├── SessionAppService.cs
│   ├── DialogueAppService.cs
│   └── AIDialogueService.cs        # 领域服务实现
├── Mappings/                        # 对象映射
│   └── EntityToDtoMapper.cs
└── DependencyInjection.cs           # DI注册
```

## 核心服务

### 会话服务（ISessionAppService）

```csharp
// 创建会话
var session = await _sessionService.CreateSessionAsync(new CreateSessionRequest
{
    Title = "龙之谷的冒险",
    Genre = "奇幻",
    Theme = "史诗冒险",
    WorldDescription = "一个充满魔法与龙的世界...",
    InitialScene = new SceneDto { Name = "村口", Description = "..." }
});

// 玩家加入
await _sessionService.JoinSessionAsync(new JoinSessionRequest
{
    SessionId = session.Id,
    UserId = "user-001",
    PlayerName = "勇者"
});

// 开始会话
await _sessionService.StartSessionAsync(session.Id);
```

### 对话服务（IDialogueAppService）

```csharp
// 发送消息
var response = await _dialogueService.SendMessageAsync(new SendMessageRequest
{
    SessionId = sessionId,
    PlayerId = playerId,
    Message = "我想和村长说话"
});

// 流式响应
await foreach (var chunk in _dialogueService.StreamMessageAsync(request))
{
    Console.Write(chunk);
}
```

## 设计原则

1. **应用服务无状态**：每个请求独立处理
2. **事务边界**：每个应用方法是一个事务单元
3. **DTO 隔离**：领域对象不直接暴露给外部
4. **依赖注入**：通过构造函数注入依赖

## 依赖

- AI-RPG.Domain
- AI-RPG.AICapabilities
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging.Abstractions
