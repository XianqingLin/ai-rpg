using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AI_RPG.Infrastructure.Services;
using AI_RPG.Infrastructure.Implementations.VectorStore;
using AI_RPG.Infrastructure.Implementations.GraphStore;
using AI_RPG.Infrastructure.Implementations.Cache;
using AI_RPG.Infrastructure.Implementations.Embedding;
using AI_RPG.Infrastructure.Plugins;

namespace AI_RPG.Infrastructure.Extensions;

/// <summary>
/// 基础设施服务注册扩展
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// 添加基础设施服务
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 注册 VectorStore
        services.Configure<QdrantOptions>(configuration.GetSection("Qdrant"));
        services.AddSingleton<IVectorStore, QdrantClient>();

        // 注册 GraphStore
        services.Configure<Neo4jOptions>(configuration.GetSection("Neo4j"));
        services.AddSingleton<IGraphStore, Neo4jClient>();

        // 注册 Cache
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.AddSingleton<ICacheService, RedisCacheService>();

        // 注册 Embedding
        services.Configure<ZhipuEmbeddingOptions>(configuration.GetSection("Zhipu"));
        services.AddHttpClient<IEmbeddingProvider, ZhipuEmbedding>();

        // 注册基础设施插件
        services.AddSingleton<VectorSearchPlugin>();
        services.AddSingleton<MemoryPlugin>();

        return services;
    }
}
