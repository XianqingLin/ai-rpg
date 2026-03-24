namespace AI_RPG.AICapabilities.Strategies.Models;

/// <summary>
/// ReAct步骤类型
/// </summary>
public enum ReActStepType
{
    /// <summary>
    /// 思考
    /// </summary>
    Thought,

    /// <summary>
    /// 行动
    /// </summary>
    Action,

    /// <summary>
    /// 观察
    /// </summary>
    Observation,

    /// <summary>
    /// 最终答案
    /// </summary>
    FinalAnswer
}

/// <summary>
/// ReAct执行步骤
/// </summary>
public sealed class ReActStep
{
    /// <summary>
    /// 步骤序号
    /// </summary>
    public required int StepNumber { get; init; }

    /// <summary>
    /// 步骤类型
    /// </summary>
    public required ReActStepType Type { get; init; }

    /// <summary>
    /// 内容
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 如果是Action，对应的工具名称
    /// </summary>
    public string? ToolName { get; init; }

    /// <summary>
    /// 如果是Action，对应的工具参数
    /// </summary>
    public Dictionary<string, object?>? ToolParameters { get; init; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// ReAct解析结果
/// </summary>
public sealed class ReActParseResult
{
    /// <summary>
    /// 思考内容
    /// </summary>
    public string? Thought { get; init; }

    /// <summary>
    /// 行动内容
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// 工具名称
    /// </summary>
    public string? ToolName { get; init; }

    /// <summary>
    /// 工具输入
    /// </summary>
    public string? ToolInput { get; init; }

    /// <summary>
    /// 最终答案
    /// </summary>
    public string? FinalAnswer { get; init; }

    /// <summary>
    /// 是否是最终答案
    /// </summary>
    public bool IsFinalAnswer => !string.IsNullOrEmpty(FinalAnswer);

    /// <summary>
    /// 是否需要执行工具
    /// </summary>
    public bool RequiresAction => !string.IsNullOrEmpty(Action) && !IsFinalAnswer;
}
