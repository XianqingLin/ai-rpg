using System.Text;
using AI_RPG.Infrastructure.Services;

namespace AI_RPG.Infrastructure.Implementations.GraphStore;

// 使用 Services 命名空间中的模型类

/// <summary>
/// Cypher 查询构建器
/// </summary>
public static class CypherQueryBuilder
{
    /// <summary>
    /// 构建创建节点查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildCreateNodeQuery(GraphNode node)
    {
        var parameters = new Dictionary<string, object>
        {
            ["id"] = node.Id,
            ["props"] = node.Properties
        };

        var query = $"CREATE (n:{SanitizeLabel(node.Label)} {{id: $id}}) SET n += $props RETURN n";
        return (query, parameters);
    }

    /// <summary>
    /// 构建批量创建节点查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildCreateNodesQuery(IEnumerable<GraphNode> nodes)
    {
        var nodeList = nodes.ToList();
        var parameters = new Dictionary<string, object>();
        var queryBuilder = new StringBuilder();

        queryBuilder.AppendLine("UNWIND $nodes AS node");
        queryBuilder.AppendLine("CREATE (n:" + SanitizeLabel(nodeList.First().Label) + " {id: node.id})");
        queryBuilder.AppendLine("SET n += node.props");
        queryBuilder.AppendLine("RETURN n");

        parameters["nodes"] = nodeList.Select(n => new Dictionary<string, object>
        {
            ["id"] = n.Id,
            ["props"] = n.Properties
        }).ToList();

        return (queryBuilder.ToString(), parameters);
    }

    /// <summary>
    /// 构建获取节点查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildGetNodeQuery(string id)
    {
        var parameters = new Dictionary<string, object> { ["id"] = id };
        var query = "MATCH (n {id: $id}) RETURN n LIMIT 1";
        return (query, parameters);
    }

    /// <summary>
    /// 构建查找节点查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildFindNodesQuery(string label, Dictionary<string, object>? properties = null)
    {
        var parameters = new Dictionary<string, object>();
        var queryBuilder = new StringBuilder();

        queryBuilder.Append($"MATCH (n:{SanitizeLabel(label)})");

        if (properties?.Count > 0)
        {
            var conditions = new List<string>();
            var index = 0;
            foreach (var (key, value) in properties)
            {
                var paramName = $"p{index}";
                conditions.Add($"n.{SanitizePropertyName(key)} = ${paramName}");
                parameters[paramName] = value;
                index++;
            }
            queryBuilder.Append(" WHERE " + string.Join(" AND ", conditions));
        }

        queryBuilder.Append(" RETURN n");
        return (queryBuilder.ToString(), parameters);
    }

    /// <summary>
    /// 构建更新节点查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildUpdateNodeQuery(string id, Dictionary<string, object> properties)
    {
        var parameters = new Dictionary<string, object>
        {
            ["id"] = id,
            ["props"] = properties
        };
        var query = "MATCH (n {id: $id}) SET n += $props RETURN n";
        return (query, parameters);
    }

    /// <summary>
    /// 构建删除节点查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildDeleteNodeQuery(string id, bool deleteRelationships)
    {
        var parameters = new Dictionary<string, object> { ["id"] = id };

        if (deleteRelationships)
        {
            var query = "MATCH (n {id: $id}) DETACH DELETE n";
            return (query, parameters);
        }
        else
        {
            var query = "MATCH (n {id: $id}) DELETE n";
            return (query, parameters);
        }
    }

    /// <summary>
    /// 构建创建关系查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildCreateRelationshipQuery(GraphRelationship relationship)
    {
        var parameters = new Dictionary<string, object>
        {
            ["sourceId"] = relationship.SourceNodeId,
            ["targetId"] = relationship.TargetNodeId,
            ["props"] = relationship.Properties
        };

        var type = SanitizeLabel(relationship.Type);
        var query = $"MATCH (a {{id: $sourceId}}), (b {{id: $targetId}}) CREATE (a)-[r:{type}]->(b) SET r += $props RETURN r";

        return (query, parameters);
    }

    /// <summary>
    /// 构建获取关系查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildGetRelationshipsQuery(string sourceNodeId, string targetNodeId, string? relationshipType = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["sourceId"] = sourceNodeId,
            ["targetId"] = targetNodeId
        };

        var relPattern = relationshipType != null
            ? $":{SanitizeLabel(relationshipType)}"
            : "";

        var query = $"MATCH (a {{id: $sourceId}})-[r{relPattern}]->(b {{id: $targetId}}) RETURN r";
        return (query, parameters);
    }

    /// <summary>
    /// 构建获取节点关系查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildGetNodeRelationshipsQuery(string nodeId, RelationshipDirection direction)
    {
        var parameters = new Dictionary<string, object> { ["id"] = nodeId };

        var pattern = direction switch
        {
            RelationshipDirection.Outgoing => "(n {id: $id})-[r]->()",
            RelationshipDirection.Incoming => "(n {id: $id})<-[r]-()",
            RelationshipDirection.Both => "(n {id: $id})-[r]-()",
            _ => "(n {id: $id})-[r]-()"
        };

        var query = $"MATCH {pattern} RETURN r";
        return (query, parameters);
    }

    /// <summary>
    /// 构建删除关系查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildDeleteRelationshipQuery(string sourceNodeId, string targetNodeId, string? relationshipType = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["sourceId"] = sourceNodeId,
            ["targetId"] = targetNodeId
        };

        var relPattern = relationshipType != null
            ? $":{SanitizeLabel(relationshipType)}"
            : "";

        var query = $"MATCH (a {{id: $sourceId}})-[r{relPattern}]->(b {{id: $targetId}}) DELETE r";
        return (query, parameters);
    }

    /// <summary>
    /// 构建最短路径查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildShortestPathQuery(string startNodeId, string endNodeId, int maxDepth)
    {
        var parameters = new Dictionary<string, object>
        {
            ["startId"] = startNodeId,
            ["endId"] = endNodeId,
            ["maxDepth"] = maxDepth
        };

        var query = """
            MATCH (start {id: $startId}), (end {id: $endId})
            CALL apoc.algo.dijkstra(start, end, null, 'weight', $maxDepth)
            YIELD path, weight
            RETURN path, weight
            LIMIT 1
            """;

        return (query, parameters);
    }

    /// <summary>
    /// 构建所有路径查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildAllPathsQuery(string startNodeId, string endNodeId, int maxDepth)
    {
        var parameters = new Dictionary<string, object>
        {
            ["startId"] = startNodeId,
            ["endId"] = endNodeId,
            ["maxDepth"] = maxDepth
        };

        var query = """
            MATCH path = (start {id: $startId})-[*1..$maxDepth]->(end {id: $endId})
            RETURN path
            """;

        return (query, parameters);
    }

    /// <summary>
    /// 构建邻居查询
    /// </summary>
    public static (string Query, Dictionary<string, object> Parameters) BuildGetNeighborsQuery(string nodeId, string? relationshipType, RelationshipDirection direction)
    {
        var parameters = new Dictionary<string, object> { ["id"] = nodeId };

        var relPattern = relationshipType != null ? $":{SanitizeLabel(relationshipType)}" : "";

        var (pattern, returnVar) = direction switch
        {
            RelationshipDirection.Outgoing => ($"(n {{id: $id}})-[r{relPattern}]->(neighbor)", "neighbor"),
            RelationshipDirection.Incoming => ($"(n {{id: $id}})<-[r{relPattern}]-(neighbor)", "neighbor"),
            RelationshipDirection.Both => ($"(n {{id: $id}})-[r{relPattern}]-(neighbor)", "neighbor"),
            _ => ($"(n {{id: $id}})-[r{relPattern}]-(neighbor)", "neighbor")
        };

        var query = $"MATCH {pattern} RETURN DISTINCT {returnVar}";
        return (query, parameters);
    }

    /// <summary>
    /// 构建清空数据库查询
    /// </summary>
    public static string BuildClearQuery() => "MATCH (n) DETACH DELETE n";

    /// <summary>
    /// 构建Ping查询
    /// </summary>
    public static string BuildPingQuery() => "RETURN 1 AS ping";

    #region 辅助方法

    /// <summary>
    /// 清理标签名称（防止注入）
    /// </summary>
    private static string SanitizeLabel(string label)
    {
        // 只允许字母、数字和下划线
        return new string(label.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    /// <summary>
    /// 清理属性名称（防止注入）
    /// </summary>
    private static string SanitizePropertyName(string name)
    {
        // 只允许字母、数字和下划线，且不能以数字开头
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }
        return sanitized;
    }

    #endregion
}
