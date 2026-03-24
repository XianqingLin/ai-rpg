namespace AI_RPG.AICapabilities.Strategies;

/// <summary>
/// 生成策略接口
/// </summary>
public interface IGenerationStrategy
{
    /// <summary>
    /// 策略名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 策略描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 执行生成策略
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <param name="context">上下文信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>生成结果</returns>
    Task<StrategyResult> ExecuteAsync(
        string input,
        StrategyContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 策略上下文
/// </summary>
public sealed class StrategyContext
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// 游戏状态JSON
    /// </summary>
    public string? GameState { get; init; }

    /// <summary>
    /// 历史记录
    /// </summary>
    public IReadOnlyList<StrategyHistoryItem> History { get; init; } = [];

    /// <summary>
    /// 可用工具
    /// </summary>
    public IReadOnlyList<string> AvailableTools { get; init; } = [];

    /// <summary>
    /// 额外参数
    /// </summary>
    public Dictionary<string, object?> Parameters { get; init; } = new();
}

/// <summary>
/// 策略历史项
/// </summary>
public sealed class StrategyHistoryItem
{
    /// <summary>
    /// 角色
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// 内容
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// 策略执行结果
/// </summary>
public sealed class StrategyResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// 输出内容
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// 使用的Token数
    /// </summary>
    public int TokenUsed { get; init; }

    /// <summary>
    /// 执行步骤数
    /// </summary>
    public int StepCount { get; init; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 额外数据
    /// </summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();
}
