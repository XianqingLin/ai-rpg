namespace AI_RPG.Infrastructure.Services;

/// <summary>
/// 图节点
/// </summary>
public sealed class GraphNode
{
    /// <summary>
    /// 节点唯一标识符
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 节点标签（类型）
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// 节点属性
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = [];
}

/// <summary>
/// 图关系
/// </summary>
public sealed class GraphRelationship
{
    /// <summary>
    /// 关系唯一标识符
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// 关系类型
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// 起始节点ID
    /// </summary>
    public required string SourceNodeId { get; init; }

    /// <summary>
    /// 目标节点ID
    /// </summary>
    public required string TargetNodeId { get; init; }

    /// <summary>
    /// 关系属性
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = [];
}

/// <summary>
/// 图路径（节点-关系链）
/// </summary>
public sealed class GraphPath
{
    /// <summary>
    /// 路径中的节点
    /// </summary>
    public List<GraphNode> Nodes { get; init; } = [];

    /// <summary>
    /// 路径中的关系
    /// </summary>
    public List<GraphRelationship> Relationships { get; init; } = [];
}

/// <summary>
/// 查询结果（节点、关系或路径）
/// </summary>
public sealed class GraphQueryResult
{
    /// <summary>
    /// 查询返回的节点
    /// </summary>
    public List<GraphNode> Nodes { get; init; } = [];

    /// <summary>
    /// 查询返回的关系
    /// </summary>
    public List<GraphRelationship> Relationships { get; init; } = [];

    /// <summary>
    /// 查询返回的路径
    /// </summary>
    public List<GraphPath> Paths { get; init; } = [];
}
