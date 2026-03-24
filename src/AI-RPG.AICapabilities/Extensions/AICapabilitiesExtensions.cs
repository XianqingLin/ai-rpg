using AI_RPG.AICapabilities.Agents;
using AI_RPG.AICapabilities.Embeddings;
using AI_RPG.AICapabilities.LLM;
using AI_RPG.AICapabilities.Prompts;
using AI_RPG.AICapabilities.Strategies;
using AI_RPG.AICapabilities.Strategies.Models;
using AI_RPG.AICapabilities.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace AI_RPG.AICapabilities.Extensions;

/// <summary>
/// AI能力层服务注册扩展
/// </summary>
public static class AICapabilitiesExtensions
{
    /// <summary>
    /// 注册AI能力层所有服务
    /// </summary>
    public static IServiceCollection AddAICapabilities(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEmbeddingServices(configuration);
        services.AddLLMServices();
        services.AddPromptServices();
        services.AddStrategyServices();
        services.AddToolServices();
        services.AddAgentServices();

        return services;
    }

    #region Embedding Services

    /// <summary>
    /// 注册嵌入服务
    /// </summary>
    public static IServiceCollection AddEmbeddingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 注册智谱Embedding配置
        services.Configure<ZhipuEmbeddingOptions>(
            configuration.GetSection("Zhipu"));

        // 注册智谱Embedding服务
        services.AddHttpClient<IEmbeddingProvider, ZhipuEmbedding>();

        return services;
    }

    /// <summary>
    /// 注册自定义EmbeddingProvider
    /// </summary>
    public static IServiceCollection AddEmbeddingProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IEmbeddingProvider
    {
        services.AddSingleton<IEmbeddingProvider, TProvider>();
        return services;
    }

    #endregion

    #region LLM Services

    /// <summary>
    /// 注册LLM服务
    /// </summary>
    public static IServiceCollection AddLLMServices(this IServiceCollection services)
    {
        // Token管理器
        services.AddSingleton<ITokenManager, TokenManager>();

        // LLM路由器
        services.AddSingleton<ILLMRouter, LLMRouter>();

        return services;
    }

    /// <summary>
    /// 注册Semantic Kernel客户端
    /// </summary>
    public static IServiceCollection AddSemanticKernelClient(
        this IServiceCollection services,
        string name,
        string modelName,
        Func<IServiceProvider, Kernel> kernelFactory)
    {
        services.AddKeyedSingleton(name, (sp, _) =>
        {
            var kernel = kernelFactory(sp);
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SemanticKernelClient>>();
            return new SemanticKernelClient(kernel, modelName, logger);
        });

        return services;
    }

    #endregion

    #region Prompt Services

    /// <summary>
    /// 注册提示工程服务
    /// </summary>
    public static IServiceCollection AddPromptServices(this IServiceCollection services)
    {
        // 提示模板管理器
        services.AddSingleton<IPromptTemplateManager, PromptTemplateManager>();

        // 上下文组装器
        services.AddScoped<IContextAssembler>(sp =>
        {
            var tokenManager = sp.GetRequiredService<ITokenManager>();
            return new ContextAssembler(tokenManager);
        });

        return services;
    }

    #endregion

    #region Strategy Services

    /// <summary>
    /// 注册生成策略服务
    /// </summary>
    public static IServiceCollection AddStrategyServices(this IServiceCollection services)
    {
        // 注册策略为Scoped，因为每个请求可能需要不同的配置
        services.AddScoped<ReActStrategy>(sp =>
        {
            var llmClient = sp.GetRequiredService<ILLMClient>();
            var toolExecutor = sp.GetRequiredService<IToolExecutor>();
            var tokenManager = sp.GetRequiredService<ITokenManager>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ReActStrategy>>();
            return new ReActStrategy(llmClient, toolExecutor, tokenManager, logger);
        });

        services.AddScoped<ReflectionStrategy>(sp =>
        {
            var llmClient = sp.GetRequiredService<ILLMClient>();
            var tokenManager = sp.GetRequiredService<ITokenManager>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ReflectionStrategy>>();
            return new ReflectionStrategy(llmClient, tokenManager, logger);
        });

        return services;
    }

    /// <summary>
    /// 注册自定义ReAct配置
    /// </summary>
    public static IServiceCollection ConfigureReAct(
        this IServiceCollection services,
        Action<ReActConfig> configure)
    {
        services.Configure(configure);
        return services;
    }

    /// <summary>
    /// 注册自定义Reflection配置
    /// </summary>
    public static IServiceCollection ConfigureReflection(
        this IServiceCollection services,
        Action<ReflectionConfig> configure)
    {
        services.Configure(configure);
        return services;
    }

    #endregion

    #region Tool Services

    /// <summary>
    /// 注册工具服务
    /// </summary>
    public static IServiceCollection AddToolServices(this IServiceCollection services)
    {
        // 工具注册表（单例）
        services.AddSingleton<IToolRegistry, ToolRegistry>();

        // 工具执行器
        services.AddScoped<IToolExecutor, ToolExecutor>();

        return services;
    }

    /// <summary>
    /// 注册领域工具构建器
    /// </summary>
    public static IServiceCollection AddDomainToolBuilder(
        this IServiceCollection services,
        Action<DomainToolBuilder> configure)
    {
        services.AddSingleton(sp =>
        {
            var toolRegistry = sp.GetRequiredService<IToolRegistry>();
            var loggerFactory = sp.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var builder = new DomainToolBuilder(toolRegistry, loggerFactory);
            configure(builder);
            return builder;
        });

        return services;
    }

    #endregion

    #region Agent Services

    /// <summary>
    /// 注册Agent服务
    /// </summary>
    public static IServiceCollection AddAgentServices(this IServiceCollection services)
    {
        // ReAct Agent工厂
        services.AddScoped<ReActAgent>(sp =>
        {
            var config = sp.GetRequiredService<AgentConfig>();
            var llmClient = sp.GetRequiredService<ILLMClient>();
            var toolExecutor = sp.GetRequiredService<IToolExecutor>();
            var tokenManager = sp.GetRequiredService<ITokenManager>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ReActAgent>>();
            return new ReActAgent(config, llmClient, toolExecutor, tokenManager, logger);
        });

        return services;
    }

    /// <summary>
    /// 注册ReAct Agent
    /// </summary>
    public static IServiceCollection AddReActAgent(
        this IServiceCollection services,
        string name,
        Action<AgentConfig> configure)
    {
        var config = new AgentConfig { Name = name };
        configure(config);
        services.AddSingleton(config);

        services.AddScoped<IAgent, ReActAgent>(sp =>
        {
            var llmClient = sp.GetRequiredService<ILLMClient>();
            var toolExecutor = sp.GetRequiredService<IToolExecutor>();
            var tokenManager = sp.GetRequiredService<ITokenManager>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ReActAgent>>();
            return new ReActAgent(config, llmClient, toolExecutor, tokenManager, logger);
        });

        return services;
    }

    #endregion
}
