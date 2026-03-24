# AI-RPG.WebAPI

AI-RPG 游戏的 WebAPI 服务层，提供 HTTP REST API 接口供前端或其他客户端调用。

## 项目结构

```
AI-RPG.WebAPI/
├── Controllers/              # API 控制器
│   ├── SessionsController.cs # 会话管理 API
│   └── DialogueController.cs # 对话交互 API
├── appsettings.json          # 生产环境配置
├── appsettings.Development.json # 开发环境配置
├── AI-RPG.WebAPI.http        # HTTP 测试文件（VS/VS Code）
├── Program.cs                # 应用入口和 DI 配置
└── README.md                 # 本文档
```

## 快速开始

### 1. 配置 Kimi API Key

编辑 `appsettings.Development.json`：

```json
{
  "Kimi": {
    "ApiKey": "your-kimi-api-key-here",
    "ModelName": "kimi-k2-5",
    "BaseUrl": "https://api.moonshot.cn/v1",
    "Temperature": 1.0,
    "MaxTokens": 4096
  }
}
```

或设置环境变量：
```bash
set KIMI_API_KEY=your-kimi-api-key-here
```

### 2. 运行项目

```bash
cd AI-RPG.WebAPI
dotnet run
```

服务将在以下地址启动：
- HTTP: `http://localhost:5134`
- HTTPS: `https://localhost:7134`（如配置）

### 3. 访问 API 文档

开发环境下访问 OpenAPI 文档：
- JSON: `http://localhost:5134/openapi/v1.json`

## API 端点

### 会话管理 (SessionsController)

| 方法 | 端点 | 描述 |
|------|------|------|
| POST | `/api/sessions` | 创建新会话 |
| GET | `/api/sessions/{sessionId}` | 获取会话详情 |
| GET | `/api/sessions/user/{userId}` | 获取用户会话列表 |
| POST | `/api/sessions/{sessionId}/join` | 玩家加入会话 |
| POST | `/api/sessions/{sessionId}/start` | 开始会话 |
| POST | `/api/sessions/{sessionId}/pause` | 暂停会话 |
| POST | `/api/sessions/{sessionId}/end` | 结束会话 |
| POST | `/api/sessions/{sessionId}/npcs` | 添加 NPC |
| DELETE | `/api/sessions/{sessionId}/npcs/{npcId}` | 移除 NPC |
| POST | `/api/sessions/{sessionId}/scene` | 切换场景 |

### 对话交互 (DialogueController)

| 方法 | 端点 | 描述 |
|------|------|------|
| POST | `/api/sessions/{sessionId}/dialogue` | 发送消息（玩家→AI） |
| GET | `/api/sessions/{sessionId}/dialogue/history` | 获取对话历史 |
| POST | `/api/sessions/{sessionId}/dialogue/stream` | 流式发送消息 |

## 使用示例

### 创建会话

```bash
curl -X POST http://localhost:5134/api/sessions \
  -H "Content-Type: application/json" \
  -d '{
    "title": "我的第一次冒险",
    "genre": "奇幻",
    "theme": "冒险",
    "worldDescription": "一个充满魔法与剑的奇幻世界",
    "initialScene": {
      "name": "起始村庄",
      "description": "一个宁静的小村庄，你是这里的冒险者"
    }
  }'
```

**响应：**
```json
{
  "id": "sess_xxx",
  "title": "我的第一次冒险",
  "status": "Preparing",
  "setting": { ... },
  "currentScene": { ... },
  "players": [],
  "npcs": [],
  "recentHistory": [],
  "createdAt": "2026-03-24T11:00:00Z"
}
```

### 玩家加入

```bash
curl -X POST http://localhost:5134/api/sessions/{sessionId}/join \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-001",
    "playerName": "勇者小明"
  }'
```

### 添加 NPC

```bash
curl -X POST http://localhost:5134/api/sessions/{sessionId}/npcs \
  -H "Content-Type: application/json" \
  -d '{
    "name": "村长",
    "appearance": "白发苍苍的老者",
    "personality": "和蔼可亲，乐于助人",
    "background": "村庄的长者"
  }'
```

### 开始会话

```bash
curl -X POST http://localhost:5134/api/sessions/{sessionId}/start
```

### 发送消息

```bash
curl -X POST http://localhost:5134/api/sessions/{sessionId}/dialogue \
  -H "Content-Type: application/json" \
  -d '{
    "playerId": "{playerId}",
    "message": "你好，村长！我想了解一下这个世界。"
  }'
```

**响应：**
```json
{
  "success": true,
  "speakerId": "npc_xxx",
  "speakerName": "村长",
  "content": "你好，年轻的冒险者！欢迎来到我们的村庄...",
  "type": "Speech",
  "timestamp": "2026-03-24T11:05:00Z"
}
```

### 获取对话历史

```bash
curl "http://localhost:5134/api/sessions/{sessionId}/dialogue/history?count=10"
```

## 核心流程

一个完整的跑团会话流程：

```
1. 创建会话 (POST /api/sessions)
   ↓
2. 添加 NPC (POST /api/sessions/{id}/npcs)
   ↓
3. 玩家加入 (POST /api/sessions/{id}/join)
   ↓
4. 开始会话 (POST /api/sessions/{id}/start)
   ↓
5. 发送消息 (POST /api/sessions/{id}/dialogue) ←→ AI 响应
   ↓
6. 结束会话 (POST /api/sessions/{id}/end)
```

## 配置说明

### appsettings.json

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `Kimi:ApiKey` | Kimi API 密钥 | - |
| `Kimi:ModelName` | 模型名称 | `kimi-k2-5` |
| `Kimi:BaseUrl` | API 基础地址 | `https://api.moonshot.cn/v1` |
| `Kimi:Temperature` | 温度参数 | `1.0` |
| `Kimi:MaxTokens` | 最大 Token 数 | `4096` |

### 环境变量

| 变量名 | 说明 |
|--------|------|
| `KIMI_API_KEY` | Kimi API 密钥（优先级高于配置文件） |
| `ASPNETCORE_ENVIRONMENT` | 运行环境（Development/Production） |

## 使用 VS Code REST Client 测试

项目包含 `AI-RPG.WebAPI.http` 文件，安装 [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) 插件后可直接在 VS Code 中发送请求。

## 依赖项目

- `AI-RPG.Domain` - 领域层（实体、值对象、领域服务）
- `AI-RPG.Application` - 应用层（应用服务、DTOs）
- `AI-RPG.AICapabilities` - AI 能力层（LLM、Agent、Tools）
- `AI-RPG.Infrastructure` - 基础设施层（仓储、缓存）

## 技术栈

- **框架**: ASP.NET Core 10.0
- **OpenAPI**: 内置 OpenAPI 支持
- **AI 集成**: Semantic Kernel + Kimi API
- **序列化**: System.Text.Json

## 注意事项

1. **API Key 安全**: 不要将真实的 API Key 提交到代码仓库
2. **会话数据**: 当前使用内存仓储，重启服务后数据会丢失
3. **并发**: 内存仓储使用读写锁，支持基本并发访问
4. **流式响应**: `/dialogue/stream` 端点使用 SSE 格式返回流式响应

## 许可证

MIT License
