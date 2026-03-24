using AI_RPG.AICapabilities.Strategies;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AI_RPG.AICapabilities.Agents;

/// <summary>
/// Agent状态
/// </summary>
public enum AgentState
{
    /// <summary>
    /// 空闲
    /// </summary>
    Idle,

    /// <summary>
    /// 思考中
    /// </summary>
    Thinking,

    /// <summary>
    /// 执行工具
    /// </summary>
    ExecutingTool,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 错误
    /// </summary>
    Error
}

/// <summary>
/// Agent配置
/// </summary>
public sealed class AgentConfig
{
    /// <summary>
    /// Agent名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Agent描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// 最大迭代次数
    /// </summary>
    public int MaxIterations { get; init; } = 10;

    /// <summary>
    /// 温度参数
    /// </summary>
    public float Temperature { get; init; } = 0.7f;

    /// <summary>
    /// 最大Token数
    /// </summary>
    public int MaxTokens { get; init; } = 2000;

    /// <summary>
    /// 可用工具列表
    /// </summary>
    public IReadOnlyList<string> Tools { get; init; } = [];
}

/// <summary>
/// Agent输入
/// </summary>
public sealed class AgentInput
{
    /// <summary>
    /// 用户消息
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 上下文数据
    /// </summary>
    public Dictionary<string, object?> Context { get; init; } = new();

    /// <summary>
    /// 是否流式输出
    /// </summary>
    public bool StreamOutput { get; init; } = false;
}

/// <summary>
/// Agent输出
/// </summary>
public sealed class AgentOutput
{
    /// <summary>
    /// 输出内容
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; } = true;

    /// <summary>
    /// 执行步骤数
    /// </summary>
    public int StepCount { get; init; }

    /// <summary>
    /// 使用的Token数
    /// </summary>
    public int TokenUsed { get; init; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// 工具调用记录
    /// </summary>
    public IReadOnlyList<ToolCallRecord> ToolCalls { get; init; } = [];

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 额外元数据
    /// </summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

/// <summary>
/// 工具调用记录
/// </summary>
public sealed class ToolCallRecord
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public required string ToolName { get; init; }

    /// <summary>
    /// 输入参数
    /// </summary>
    public string? Input { get; init; }

    /// <summary>
    /// 输出结果
    /// </summary>
    public string? Output { get; init; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; } = true;

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Agent接口
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Agent配置
    /// </summary>
    AgentConfig Config { get; }

    /// <summary>
    /// 当前状态
    /// </summary>
    AgentState State { get; }

    /// <summary>
    /// 执行Agent
    /// </summary>
    Task<AgentOutput> RunAsync(AgentInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 流式执行Agent
    /// </summary>
    IAsyncEnumerable<string> RunStreamingAsync(AgentInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置Agent状态
    /// </summary>
    void Reset();
}
