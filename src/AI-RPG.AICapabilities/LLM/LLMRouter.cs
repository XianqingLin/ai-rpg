using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AI_RPG.AICapabilities.LLM;

/// <summary>
/// 多模型路由配置
/// </summary>
public sealed class ModelConfig
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public required string ModelName { get; init; }

    /// <summary>
    /// 模型别名（用于路由选择）
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// 优先级（数字越小优先级越高）
    /// </summary>
    public int Priority { get; init; } = 1;

    /// <summary>
    /// 成本等级（1-10，数字越大成本越高）
    /// </summary>
    public int CostLevel { get; init; } = 5;

    /// <summary>
    /// 质量等级（1-10，数字越大质量越高）
    /// </summary>
    public int QualityLevel { get; init; } = 5;

    /// <summary>
    /// 最大Token数
    /// </summary>
    public int MaxTokens { get; init; } = 4096;

    /// <summary>
    /// 适用任务类型
    /// </summary>
    public IReadOnlyList<string> SuitableTasks { get; init; } = [];
}

/// <summary>
/// 模型路由选项
/// </summary>
public enum ModelRouteOption
{
    /// <summary>
    /// 默认模型
    /// </summary>
    Default,

    /// <summary>
    /// 成本最优
    /// </summary>
    CostOptimized,

    /// <summary>
    /// 质量最优
    /// </summary>
    QualityOptimized,

    /// <summary>
    /// 速度最优
    /// </summary>
    SpeedOptimized
}

/// <summary>
/// LLM多模型路由器
/// </summary>
public interface ILLMRouter
{
    /// <summary>
    /// 注册模型
    /// </summary>
    void RegisterModel(string alias, ModelConfig config, Kernel kernel);

    /// <summary>
    /// 获取指定别名的客户端
    /// </summary>
    ILLMClient? GetClient(string alias);

    /// <summary>
    /// 根据任务类型和路由选项选择最佳客户端
    /// </summary>
    ILLMClient SelectClient(string taskType, ModelRouteOption option = ModelRouteOption.Default);

    /// <summary>
    /// 获取所有已注册的模型别名
    /// </summary>
    IReadOnlyList<string> GetRegisteredModels();
}

/// <summary>
/// LLM多模型路由器实现
/// </summary>
public sealed class LLMRouter : ILLMRouter
{
    private readonly Dictionary<string, (ModelConfig Config, Kernel Kernel)> _models = new();
    private readonly Dictionary<string, ILLMClient> _clients = new();
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<LLMRouter> _logger;

    public LLMRouter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<LLMRouter>();
    }

    public void RegisterModel(string alias, ModelConfig config, Kernel kernel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(kernel);

        if (_models.ContainsKey(alias))
        {
            throw new InvalidOperationException($"Model with alias '{alias}' is already registered");
        }

        _models[alias] = (config, kernel);
        
        var clientLogger = _loggerFactory.CreateLogger<SemanticKernelClient>();
        _clients[alias] = new SemanticKernelClient(kernel, config.ModelName, clientLogger);

        _logger.LogInformation("Registered model '{Alias}' with model '{ModelName}'", alias, config.ModelName);
    }

    public ILLMClient? GetClient(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        
        _clients.TryGetValue(alias, out var client);
        return client;
    }

    public ILLMClient SelectClient(string taskType, ModelRouteOption option = ModelRouteOption.Default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);

        if (_models.Count == 0)
        {
            throw new InvalidOperationException("No models are registered");
        }

        var candidates = _models
            .Where(m => m.Value.Config.SuitableTasks.Count == 0 || 
                        m.Value.Config.SuitableTasks.Contains(taskType))
            .ToList();

        if (candidates.Count == 0)
        {
            // 如果没有专门适合该任务的模型，使用所有模型
            candidates = _models.ToList();
        }

        var selected = option switch
        {
            ModelRouteOption.CostOptimized => candidates.OrderBy(m => m.Value.Config.CostLevel)
                                                         .ThenBy(m => m.Value.Config.Priority)
                                                         .First(),
            ModelRouteOption.QualityOptimized => candidates.OrderByDescending(m => m.Value.Config.QualityLevel)
                                                          .ThenBy(m => m.Value.Config.Priority)
                                                          .First(),
            ModelRouteOption.SpeedOptimized => candidates.OrderBy(m => m.Value.Config.MaxTokens)
                                                        .ThenBy(m => m.Value.Config.Priority)
                                                        .First(),
            _ => candidates.OrderBy(m => m.Value.Config.Priority).First()
        };

        _logger.LogDebug("Selected model '{Alias}' for task '{TaskType}' with option {Option}",
            selected.Key, taskType, option);

        return _clients[selected.Key];
    }

    public IReadOnlyList<string> GetRegisteredModels()
    {
        return _models.Keys.ToList();
    }
}
