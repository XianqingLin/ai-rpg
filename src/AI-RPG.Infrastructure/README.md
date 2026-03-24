# AI-RPG.Infrastructure

AI-RPG 游戏的基础设施层，提供数据存储、缓存等通用技术服务。

## 项目结构

```
AI-RPG.Infrastructure/
├── Services/                      # 服务接口和模型
│   ├── IVectorStore.cs           # 向量数据库接口
│   ├── IGraphStore.cs            # 图数据库接口
│   ├── ICacheService.cs          # 缓存服务接口
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
│   └── Cache/                    # Redis 实现
│       ├── RedisCacheService.cs
│       └── RedisOptions.cs
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
```

## 依赖包

| 包名 | 版本 | 用途 |
|------|------|------|
| Qdrant.Client | 1.17.0 | 向量数据库 |
| Neo4j.Driver | 5.27.0 | 图数据库 |
| StackExchange.Redis | 2.8.24 | Redis 客户端 |
| Microsoft.Extensions.* | 9.0.2 | 微软扩展库 |

## 迁移说明

### 文本嵌入服务 (IEmbeddingProvider) → AI-RPG.AICapabilities 层

⚠️ **已迁移到 AI-RPG.AICapabilities 层**

文本嵌入功能已从Infrastructure层迁移到AICapabilities层，原因：
- 文本嵌入是AI核心能力，与LLM紧密相关
- 需要与Token管理、模型路由统一管理
- 应用层通过AICapabilities层统一访问AI能力

**迁移后的使用方式：**
```csharp
// 在Program.cs中
builder.Services.AddAICapabilities(builder.Configuration);

// 或者单独注册
services.AddEmbeddingServices(configuration);
```

### Semantic Kernel 插件 → AI-RPG.AICapabilities 层

⚠️ **已迁移到 AI-RPG.AICapabilities 层**

原`VectorSearchPlugin`和`MemoryPlugin`已迁移到AICapabilities层，作为AI能力的组成部分。

## 架构说明

### 分层设计

```
┌─────────────────────────────────────┐
│         Application Layer           │
│    (AI-RPG.Application)             │
├─────────────────────────────────────┤
│      AI-RPG.AICapabilities          │  ← AI能力层（LLM、嵌入、插件）
├─────────────────────────────────────┤
│         Infrastructure Layer        │  ← 本层（数据存储、缓存）
│  - Services: 接口定义               │
│  - Implementations: 技术实现        │
└─────────────────────────────────────┘
```

### 设计原则

1. **接口与实现分离** - Services 定义接口，Implementations 提供具体实现
2. **无业务逻辑** - 本层只提供技术能力，业务逻辑在上层实现
3. **技术中立** - 不依赖特定AI框架，纯数据存储和访问

## 许可证

MIT License
