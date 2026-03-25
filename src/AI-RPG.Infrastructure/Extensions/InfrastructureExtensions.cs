using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using AI_RPG.Domain.Repositories;
using AI_RPG.Infrastructure.Services;
using AI_RPG.Infrastructure.Implementations.VectorStore;
using AI_RPG.Infrastructure.Implementations.GraphStore;
using AI_RPG.Infrastructure.Implementations.Cache;
using AI_RPG.Infrastructure.Data;
using AI_RPG.Infrastructure.Repositories;

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

        // 注册数据库配置
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));

        // 注册仓储
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
