namespace AI_RPG.Infrastructure.Services;

/// <summary>
/// 向量点
/// </summary>
public sealed class VectorPoint
{
    /// <summary>
    /// 点ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 向量数据
    /// </summary>
    public required ReadOnlyMemory<float> Vector { get; init; }

    /// <summary>
    /// 负载数据（附加属性）
    /// </summary>
    public Dictionary<string, object> Payload { get; init; } = [];
}

/// <summary>
/// 搜索结果
/// </summary>
public sealed class SearchResult
{
    /// <summary>
    /// 点ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 相似度分数
    /// </summary>
    public required float Score { get; init; }

    /// <summary>
    /// 向量数据
    /// </summary>
    public ReadOnlyMemory<float>? Vector { get; init; }

    /// <summary>
    /// 负载数据
    /// </summary>
    public Dictionary<string, object> Payload { get; init; } = [];
}
