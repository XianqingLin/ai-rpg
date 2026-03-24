using AI_RPG.Infrastructure.Services;

namespace AI_RPG.AICapabilities.Prompts;

/// <summary>
/// 上下文片段类型
/// </summary>
public enum ContextPieceType
{
    /// <summary>
    /// 系统指令
    /// </summary>
    SystemInstruction,

    /// <summary>
    /// 对话历史
    /// </summary>
    ConversationHistory,

    /// <summary>
    /// 知识检索结果
    /// </summary>
    RetrievedKnowledge,

    /// <summary>
    /// 游戏状态
    /// </summary>
    GameState,

    /// <summary>
    /// 玩家输入
    /// </summary>
    UserInput,

    /// <summary>
    /// 工具执行结果
    /// </summary>
    ToolResult,

    /// <summary>
    /// 自定义内容
    /// </summary>
    Custom
}

/// <summary>
/// 上下文片段
/// </summary>
public sealed class ContextPiece
{
    /// <summary>
    /// 片段类型
    /// </summary>
    public required ContextPieceType Type { get; init; }

    /// <summary>
    /// 内容
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 优先级（数字越大优先级越高，越靠前）
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// 预估Token数
    /// </summary>
    public int? EstimatedTokens { get; init; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

/// <summary>
/// 上下文组装选项
/// </summary>
public sealed class ContextAssemblyOptions
{
    /// <summary>
    /// 最大Token数限制
    /// </summary>
    public int MaxTokens { get; set; } = 4000;

    /// <summary>
    /// 系统指令优先级（默认最高）
    /// </summary>
    public int SystemInstructionPriority { get; set; } = 100;

    /// <summary>
    /// 玩家输入优先级
    /// </summary>
    public int UserInputPriority { get; set; } = 90;

    /// <summary>
    /// 是否包含对话历史
    /// </summary>
    public bool IncludeConversationHistory { get; set; } = true;

    /// <summary>
    /// 对话历史最大轮数
    /// </summary>
    public int MaxConversationRounds { get; set; } = 10;

    /// <summary>
    /// 知识检索结果最大数量
    /// </summary>
    public int MaxRetrievedKnowledge { get; set; } = 5;
}

/// <summary>
/// 上下文组装器接口
/// </summary>
public interface IContextAssembler
{
    /// <summary>
    /// 添加上下文片段
    /// </summary>
    void AddPiece(ContextPiece piece);

    /// <summary>
    /// 组装最终提示
    /// </summary>
    string Assemble(ContextAssemblyOptions? options = null);

    /// <summary>
    /// 清空所有片段
    /// </summary>
    void Clear();

    /// <summary>
    /// 获取当前所有片段
    /// </summary>
    IReadOnlyList<ContextPiece> GetPieces();
}

/// <summary>
/// 上下文组装器实现
/// </summary>
public sealed class ContextAssembler : IContextAssembler
{
    private readonly List<ContextPiece> _pieces = new();
    private readonly LLM.ITokenManager _tokenManager;

    public ContextAssembler(LLM.ITokenManager tokenManager)
    {
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    public void AddPiece(ContextPiece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);
        _pieces.Add(piece);
    }

    public string Assemble(ContextAssemblyOptions? options = null)
    {
        options ??= new ContextAssemblyOptions();

        // 按优先级排序
        var sortedPieces = _pieces.OrderByDescending(p => p.Priority).ToList();

        var result = new System.Text.StringBuilder();
        var currentTokens = 0;

        foreach (var piece in sortedPieces)
        {
            var tokens = piece.EstimatedTokens ?? _tokenManager.EstimateTokens(piece.Content);
            
            // 检查是否超出Token限制
            if (currentTokens + tokens > options.MaxTokens)
            {
                // 如果是高优先级片段，尝试截断
                if (piece.Priority >= options.UserInputPriority)
                {
                    var remainingTokens = options.MaxTokens - currentTokens;
                    if (remainingTokens > 50) // 至少保留50个token
                    {
                        var truncated = TruncateContent(piece.Content, remainingTokens);
                        result.AppendLine(truncated);
                    }
                }
                continue;
            }

            result.AppendLine(piece.Content);
            currentTokens += tokens;
        }

        return result.ToString().Trim();
    }

    public void Clear()
    {
        _pieces.Clear();
    }

    public IReadOnlyList<ContextPiece> GetPieces()
    {
        return _pieces.ToList();
    }

    private string TruncateContent(string content, int maxTokens)
    {
        // 简单估算：按字符数截断
        var estimatedChars = (int)(maxTokens * 2.5);
        if (content.Length <= estimatedChars)
            return content;

        return content[..estimatedChars] + "...";
    }
}
