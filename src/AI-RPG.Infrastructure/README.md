# AI-RPG.Infrastructure

AI-RPG 游戏的基础设施层，提供数据存储、缓存、嵌入等通用技术服务。

## 项目结构

```
AI-RPG.Infrastructure/
├── Services/                      # 服务接口和模型
│   ├── IVectorStore.cs           # 向量数据库接口
│   ├── IGraphStore.cs            # 图数据库接口
│   ├── ICacheService.cs          # 缓存服务接口
│   ├── IEmbeddingProvider.cs     # 嵌入服务接口
│   ├── VectorStoreModels.cs      # 向量相关模型 (VectorPoint, SearchResult)
│   └── GraphStoreModels.cs       # 图数据库模型 (GraphNode, GraphRelationship, etc.)
├── Implementations/               # 服务实现
│   ├── VectorStore/              # Qdrant 实现
│   │   ├── QdrantClient.cs
│   │   ├── QdrantOptions.cs
│   │   ├── QdrantConverters.cs
│   │   └── QdrantFilterBuilder.cs
│   ├── GraphStore/               # Neo4j 实现
│   │   ├── Neo4jClient.cs
│   │   ├── Neo4jOptions.cs
│   │   └── CypherQueryBuilder.cs
│   ├── Cache/                    # Redis 实现
│   │   ├── RedisCacheService.cs
│   │   └── RedisOptions.cs
│   └── Embedding/                # 智谱 AI 嵌入实现
│       └── ZhipuEmbedding.cs
├── Plugins/                       # Semantic Kernel 插件
│   ├── VectorSearchPlugin.cs     # 向量检索插件
│   └── MemoryPlugin.cs           # 对话历史管理插件
└── Extensions/
    └── InfrastructureExtensions.cs # DI 注册扩展
```

## 核心功能

### 1. 向量存储 (IVectorStore)

基于 Qdrant 的向量数据库：

```csharp
// 注入使用
public class MyService
{
    private readonly IVectorStore _vectorStore;
    
    public MyService(IVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }
    
    public async Task StoreDocumentAsync(string id, float[] vector, string text)
    {
        await _vectorStore.UpsertAsync("my-collection", new VectorPoint
        {
            Id = id,
            Vector = vector,
            Payload = new Dictionary<string, object> { ["text"] = text }
        });
    }
    
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryVector)
    {
        return await _vectorStore.SearchAsync("my-collection", queryVector, limit: 5);
    }
}
```

### 2. 图数据库 (IGraphStore)

基于 Neo4j 的图数据存储：

```csharp
// 创建节点
await _graphStore.CreateNodeAsync(new GraphNode
{
    Id = "player-001",
    Label = "Player",
    Properties = new Dictionary<string, object> { ["name"] = "Hero" }
});

// 创建关系
await _graphStore.CreateRelationshipAsync(new GraphRelationship
{
    Type = "OWNS",
    SourceNodeId = "player-001",
    TargetNodeId = "item-001"
});

// 查找路径
var path = await _graphStore.FindShortestPathAsync("player-001", "npc-001", maxDepth: 5);
```

### 3. 缓存服务 (ICacheService)

基于 Redis 的分布式缓存：

```csharp
// 基础操作
await _cache.SetAsync("key", value, TimeSpan.FromMinutes(30));
var value = await _cache.GetAsync<MyType>("key");

// 分布式锁
var acquired = await _cache.AcquireLockAsync("resource-lock", lockValue, TimeSpan.FromSeconds(30));
if (acquired)
{
    try { /* 执行业务逻辑 */ }
    finally { await _cache.ReleaseLockAsync("resource-lock", lockValue); }
}
```

### 4. 文本嵌入 (IEmbeddingProvider)

智谱 AI 嵌入服务：

```csharp
// 生成单个文本的嵌入
var embedding = await _embeddingProvider.GenerateEmbeddingAsync("这是一段文本");

// 批量生成
var embeddings = await _embeddingProvider.GenerateEmbeddingsAsync(new[] { "文本1", "文本2" });
```

### 5. Semantic Kernel 插件

基础设施层提供的通用 SK 插件：

```csharp
// VectorSearchPlugin - 向量检索
var results = await vectorSearchPlugin.SearchSimilarAsync(
    collectionName: "game-lore",
    query: "关于龙的历史",
    topK: 3);

// MemoryPlugin - 对话历史管理
var history = await memoryPlugin.LoadConversationHistoryAsync("session-001");
await memoryPlugin.SaveConversationTurnAsync("session-001", userMessage, assistantMessage);
```

## 配置

### appsettings.json

```json
{
  "Qdrant": {
    "Uri": "http://localhost:6333",
    "ApiKey": null
  },
  "Neo4j": {
    "Uri": "bolt://localhost:7687",
    "Username": "neo4j",
    "Password": "password",
    "Database": null
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DefaultDatabase": 0,
    "KeyPrefix": "ai-rpg:"
  },
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
using AI_RPG.Infrastructure.Extensions;

// 注册所有基础设施服务
builder.Services.AddInfrastructure(builder.Configuration);

// 或者分别注册
services.AddSingleton<IVectorStore, QdrantClient>();
services.AddSingleton<IGraphStore, Neo4jClient>();
services.AddSingleton<ICacheService, RedisCacheService>();
services.AddSingleton<IEmbeddingProvider, ZhipuEmbedding>();
```

## 与 Semantic Kernel 集成

基础设施层提供通用插件，应用层可以将其注册到 SK Kernel：

```csharp
// 在应用层（如 AI-RPG.Application）
public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
{
    // 先注册基础设施
    services.AddInfrastructure(config);
    
    // 创建 Kernel 并添加基础设施插件
    services.AddSingleton<Kernel>(sp =>
    {
        var builder = Kernel.CreateBuilder();
        
        // 添加 LLM
        builder.AddOpenAIChatCompletion("gpt-4", config["OpenAI:ApiKey"]!);
        
        // 添加基础设施插件
        builder.Plugins.AddFromObject(sp.GetRequiredService<VectorSearchPlugin>(), "VectorSearch");
        builder.Plugins.AddFromObject(sp.GetRequiredService<MemoryPlugin>(), "Memory");
        
        return builder.Build();
    });
    
    return services;
}
```

## 依赖包

| 包名 | 版本 | 用途 |
|------|------|------|
| Microsoft.SemanticKernel | 1.40.0 | AI 能力框架 |
| OpenAI | 2.2.0-beta.1 | OpenAI API |
| Qdrant.Client | 1.17.0 | 向量数据库 |
| Neo4j.Driver | 5.27.0 | 图数据库 |
| StackExchange.Redis | 2.8.24 | Redis 客户端 |
| Microsoft.Extensions.* | 9.0.2 | 微软扩展库 |

## 架构说明

### 分层设计

```
┌─────────────────────────────────────┐
│         Application Layer           │  ← 业务插件（带 SK 特性）
│    (AI-RPG.Application)             │
├─────────────────────────────────────┤
│         Infrastructure Layer        │  ← 本层
│  - Services: 接口定义               │
│  - Implementations: 技术实现        │
│  - Plugins: 通用 SK 插件（无特性）  │
└─────────────────────────────────────┘
```

### 设计原则

1. **接口与实现分离** - Services 定义接口，Implementations 提供具体实现
2. **插件化设计** - Plugins 提供通用能力，应用层决定如何组装
3. **无业务逻辑** - 本层只提供技术能力，业务逻辑在上层实现
4. **Semantic Kernel 友好** - 提供 SK 插件，支持 AI 应用开发

## 许可证

MIT License
