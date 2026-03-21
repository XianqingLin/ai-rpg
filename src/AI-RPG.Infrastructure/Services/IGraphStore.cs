namespace AI_RPG.Infrastructure.Services;

/// <summary>
/// 图数据库接口
/// </summary>
public interface IGraphStore
{
    #region 节点操作

    /// <summary>
    /// 创建节点
    /// </summary>
    /// <param name="node">节点信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CreateNodeAsync(GraphNode node, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量创建节点
    /// </summary>
    /// <param name="nodes">节点列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CreateNodesAsync(IEnumerable<GraphNode> nodes, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取节点
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>节点，不存在时返回null</returns>
    Task<GraphNode?> GetNodeAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标签和属性查找节点
    /// </summary>
    /// <param name="label">节点标签</param>
    /// <param name="properties">匹配属性</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<GraphNode>> FindNodesAsync(string label, Dictionary<string, object>? properties = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新节点属性
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <param name="properties">更新的属性</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateNodeAsync(string id, Dictionary<string, object> properties, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除节点
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <param name="deleteRelationships">是否同时删除关联关系</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> DeleteNodeAsync(string id, bool deleteRelationships = false, CancellationToken cancellationToken = default);

    #endregion

    #region 关系操作

    /// <summary>
    /// 创建关系
    /// </summary>
    /// <param name="relationship">关系信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CreateRelationshipAsync(GraphRelationship relationship, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量创建关系
    /// </summary>
    /// <param name="relationships">关系列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CreateRelationshipsAsync(IEnumerable<GraphRelationship> relationships, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取节点间的关系
    /// </summary>
    /// <param name="sourceNodeId">起始节点ID</param>
    /// <param name="targetNodeId">目标节点ID</param>
    /// <param name="relationshipType">关系类型（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<GraphRelationship>> GetRelationshipsAsync(string sourceNodeId, string targetNodeId, string? relationshipType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取节点的所有关系
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <param name="direction">关系方向（出、入或双向）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<GraphRelationship>> GetNodeRelationshipsAsync(string nodeId, RelationshipDirection direction = RelationshipDirection.Both, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除关系
    /// </summary>
    /// <param name="sourceNodeId">起始节点ID</param>
    /// <param name="targetNodeId">目标节点ID</param>
    /// <param name="relationshipType">关系类型（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> DeleteRelationshipAsync(string sourceNodeId, string targetNodeId, string? relationshipType = null, CancellationToken cancellationToken = default);

    #endregion

    #region 查询操作

    /// <summary>
    /// 执行原始Cypher查询
    /// </summary>
    /// <param name="cypher">Cypher查询语句</param>
    /// <param name="parameters">查询参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<GraphQueryResult> ExecuteQueryAsync(string cypher, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查找最短路径
    /// </summary>
    /// <param name="startNodeId">起始节点ID</param>
    /// <param name="endNodeId">目标节点ID</param>
    /// <param name="maxDepth">最大深度</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<GraphPath?> FindShortestPathAsync(string startNodeId, string endNodeId, int maxDepth = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查找所有路径
    /// </summary>
    /// <param name="startNodeId">起始节点ID</param>
    /// <param name="endNodeId">目标节点ID</param>
    /// <param name="maxDepth">最大深度</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<GraphPath>> FindAllPathsAsync(string startNodeId, string endNodeId, int maxDepth = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// 邻居查询（获取相邻节点）
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <param name="relationshipType">关系类型过滤（可选）</param>
    /// <param name="direction">关系方向</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<GraphNode>> GetNeighborsAsync(string nodeId, string? relationshipType = null, RelationshipDirection direction = RelationshipDirection.Both, CancellationToken cancellationToken = default);

    #endregion

    #region 图操作

    /// <summary>
    /// 清空数据库
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查数据库连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// 关系方向
/// </summary>
public enum RelationshipDirection
{
    /// <summary>
    /// 出站关系（从节点指向外）
    /// </summary>
    Outgoing,

    /// <summary>
    /// 入站关系（指向节点）
    /// </summary>
    Incoming,

    /// <summary>
    /// 双向关系
    /// </summary>
    Both
}
