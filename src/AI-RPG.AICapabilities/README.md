# AI-RPG.AICapabilities

AI-RPG 的 AI 能力层，封装所有与 LLM 交互、生成策略、提示工程、工具执行和智能体相关的功能。

## 项目定位

位于 **Infrastructure 层** 和 **Domain 层** 之间，为上层提供高层次的 AI 能力抽象。

```
┌─────────────────────────────────────┐
│         Application Layer           │
│    (AI-RPG.Application)             │
├─────────────────────────────────────┤
│      AI-RPG.AICapabilities          │  ← 本层
│  - Agent、LLM交互、生成策略          │
│  - 提示工程、工具执行、文本嵌入       │
├─────────────────────────────────────┤
│         Infrastructure Layer        │
│    (AI-RPG.Infrastructure)          │
└─────────────────────────────────────┘
```

## 项目结构

```
AI-RPG.AICapabilities/
├── Agents/                          # 智能体抽象
│   ├── IAgent.cs
│   └── ReActAgent.cs
├── LLM/                             # LLM交互
│   ├── ILLMClient.cs
│   ├── SemanticKernelClient.cs
│   ├── LLMRouter.cs
│   └── TokenManager.cs
├── Prompts/                         # 提示工程
│   ├── IPromptTemplate.cs
│   ├── IContextAssembler.cs
│   └── Templates/
├── Strategies/                      # 生成策略
│   ├── IGenerationStrategy.cs
│   ├── ReActStrategy.cs
│   └── ReflectionStrategy.cs
├── Tools/                           # 工具执行
│   ├── IToolRegistry.cs
│   ├── IToolExecutor.cs
│   └── DomainToolAdapter.cs
├── Embeddings/                      # 文本嵌入（从Infrastructure迁移）
│   ├── IEmbeddingProvider.cs
│   ├── ZhipuEmbedding.cs
│   └── ZhipuEmbeddingOptions.cs
└── Extensions/
    └── AICapabilitiesExtensions.cs  # DI注册扩展
```

## 核心功能

### 1. Agent (IAgent)

ReAct Agent实现，支持思考-行动-观察循环：

```csharp
// 注册Agent
services.AddReActAgent("GameMaster", config =>
{
    config.Description = "游戏主持人Agent";
    config.SystemPrompt = "你是一个游戏主持人...";
    config.Tools = ["SearchLore", "UpdateGameState"];
    config.MaxIterations = 5;
});

// 使用Agent
var agent = serviceProvider.GetRequiredService<IAgent>();
var output = await agent.RunAsync(new AgentInput
{
    Message = "玩家想要探索森林",
    SessionId = "session-001"
});

Console.WriteLine(output.Content);
```

### 2. 文本嵌入 (IEmbeddingProvider)

从 Infrastructure 层迁移过来的嵌入服务：

```csharp
// 生成单个文本的嵌入
var embedding = await _embeddingProvider.GenerateEmbeddingAsync("这是一段文本");

// 批量生成
var embeddings = await _embeddingProvider.GenerateEmbeddingsAsync(new[] { "文本1", "文本2" });
```

### 3. LLM客户端 (ILLMClient)

统一的LLM调用接口：

```csharp
// 发送单轮消息
var response = await _llmClient.SendMessageAsync("你好");

// 发送聊天历史
var chatHistory = new ChatHistory();
chatHistory.AddUserMessage("你好");
var response = await _llmClient.SendChatAsync(chatHistory);

// 流式输出
await foreach (var chunk in _llmClient.SendStreamingAsync(chatHistory))
{
    Console.Write(chunk.Content);
}
```

### 4. 多模型路由 (ILLMRouter)

根据任务类型和策略选择最佳模型：

```csharp
// 注册多个模型
_router.RegisterModel("gpt-4", new ModelConfig { 
    ModelName = "gpt-4", 
    Alias = "high-quality",
    QualityLevel = 10,
    CostLevel = 10
}, kernel1);

_router.RegisterModel("gpt-3.5", new ModelConfig { 
    ModelName = "gpt-3.5-turbo", 
    Alias = "cost-effective",
    QualityLevel = 6,
    CostLevel = 3
}, kernel2);

// 根据任务选择模型
var client = _router.SelectClient("dialogue", ModelRouteOption.QualityOptimized);
var response = await client.SendMessageAsync("...");
```

### 5. 生成策略 (IGenerationStrategy)

```csharp
// ReAct策略
var reActStrategy = new ReActStrategy(llmClient, toolExecutor, tokenManager, logger);
var result = await reActStrategy.ExecuteAsync("查询任务状态", strategyContext);

// 反思策略
var reflectionStrategy = new ReflectionStrategy(llmClient, tokenManager, logger);
var result = await reflectionStrategy.ExecuteAsync("生成剧情", strategyContext);
```

### 6. 工具执行 (IToolExecutor)

```csharp
// 执行单个工具
var result = await _toolExecutor.ExecuteAsync("SearchLore", "{ \"query\": \"龙的历史\" }");

// 批量执行
var results = await _toolExecutor.ExecuteBatchAsync(new[]
{
    new ToolExecutionRequest { ToolName = "Tool1", Parameters = ... },
    new ToolExecutionRequest { ToolName = "Tool2", Parameters = ... }
});
```

### 7. 领域服务适配 (DomainToolAdapter)

将领域服务方法包装为AI工具：

```csharp
// 注册领域服务方法为工具
services.AddDomainToolBuilder(builder =>
{
    builder.RegisterFromService(gameStateService, "UpdateState", "GetState");
    builder.RegisterFromService(loreService);
});
```

## 配置

### appsettings.json

```json
{
  "Zhipu": {
    "ApiKey": "your-api-key",
    "Model": "embedding-3",
    "Dimensions": 512
  }
}
```

## 依赖注入注册

```csharp
// Program.cs
using AI_RPG.AICapabilities.Extensions;

// 注册所有AI能力服务
builder.Services.AddAICapabilities(builder.Configuration);

// 注册ReAct Agent
builder.Services.AddReActAgent("GameMaster", config =>
{
    config.SystemPrompt = "你是一个游戏主持人...";
    config.Tools = ["SearchLore", "UpdateGameState"];
});

// 或者单独注册
services.AddEmbeddingServices(configuration);
services.AddLLMServices();
services.AddPromptServices();
services.AddStrategyServices();
services.AddToolServices();
services.AddAgentServices();
```

## 与 Semantic Kernel 集成

```csharp
// 创建 Kernel
services.AddSingleton<Kernel>(sp =>
{
    var builder = Kernel.CreateBuilder();
    
    // 添加 LLM
    builder.AddOpenAIChatCompletion("gpt-4", config["OpenAI:ApiKey"]!);
    
    return builder.Build();
});

// 注册LLM客户端
services.AddScoped<ILLMClient>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    var logger = sp.GetRequiredService<ILogger<SemanticKernelClient>>();
    return new SemanticKernelClient(kernel, "gpt-4", logger);
});
```

## 依赖包

| 包名 | 版本 | 用途 |
|------|------|------|
| Microsoft.SemanticKernel | 1.40.0 | AI 能力框架 |
| Microsoft.SemanticKernel.PromptTemplates.Handlebars | 1.40.0 | Handlebars 模板 |
| Microsoft.Extensions.* | 9.0.2 | 微软扩展库 |

## 许可证

MIT License
