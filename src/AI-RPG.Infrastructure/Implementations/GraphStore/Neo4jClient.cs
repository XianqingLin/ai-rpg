using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using AI_RPG.Infrastructure.Services;

namespace AI_RPG.Infrastructure.Implementations.GraphStore;

// GraphNode, GraphRelationship, GraphPath, GraphQueryResult 来自 Services 命名空间

/// <summary>
/// Neo4j 图数据库客户端实现
/// </summary>
public sealed class Neo4jClient : IGraphStore, IDisposable
{
    private readonly IDriver _driver;
    private readonly ILogger<Neo4jClient> _logger;
    private readonly Neo4jOptions _options;

    /// <summary>
    /// 初始化 Neo4j 客户端
    /// </summary>
    public Neo4jClient(IOptions<Neo4jOptions> options, ILogger<Neo4jClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var authToken = AuthTokens.Basic(_options.Username, _options.Password);

        _driver = GraphDatabase.Driver(
            _options.Uri,
            authToken,
            config => config
                .WithConnectionTimeout(TimeSpan.FromSeconds(_options.ConnectionTimeout))
                .WithMaxConnectionPoolSize(_options.MaxConnectionPoolSize)
                .WithEncryptionLevel(_options.Encrypted ? EncryptionLevel.Encrypted : EncryptionLevel.None));

        _logger.LogInformation("Neo4j client initialized: {Uri}", _options.Uri);
    }

    #region 节点操作

    public async Task CreateNodeAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        ValidateNode(node);

        var (query, parameters) = CypherQueryBuilder.BuildCreateNodeQuery(node);
        await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
        _logger.LogDebug("Created Neo4j node: {NodeId} with label {Label}", node.Id, node.Label);
    }

    public async Task CreateNodesAsync(IEnumerable<GraphNode> nodes, CancellationToken cancellationToken = default)
    {
        var nodeList = nodes.ToList();
        if (nodeList.Count == 0) return;

        var (query, parameters) = CypherQueryBuilder.BuildCreateNodesQuery(nodeList);
        await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
        _logger.LogDebug("Created {Count} Neo4j nodes", nodeList.Count);
    }

    public async Task<GraphNode?> GetNodeAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var (query, parameters) = CypherQueryBuilder.BuildGetNodeQuery(id);
        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Nodes.FirstOrDefault();
    }

    public async Task<IReadOnlyList<GraphNode>> FindNodesAsync(string label, Dictionary<string, object>? properties = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        var (query, parameters) = CypherQueryBuilder.BuildFindNodesQuery(label, properties);
        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Nodes;
    }

    public async Task UpdateNodeAsync(string id, Dictionary<string, object> properties, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(properties);

        if (properties.Count == 0) return;

        var (query, parameters) = CypherQueryBuilder.BuildUpdateNodeQuery(id, properties);
        await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
        _logger.LogDebug("Updated Neo4j node: {NodeId}", id);
    }

    public async Task<bool> DeleteNodeAsync(string id, bool deleteRelationships = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var (query, parameters) = CypherQueryBuilder.BuildDeleteNodeQuery(id, deleteRelationships);
        try
        {
            await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
            _logger.LogDebug("Deleted Neo4j node: {NodeId}", id);
            return true;
        }
        catch (ClientException)
        {
            _logger.LogWarning("Cannot delete node {NodeId}: it still has relationships", id);
            return false;
        }
    }

    #endregion

    #region 关系操作

    public async Task CreateRelationshipAsync(GraphRelationship relationship, CancellationToken cancellationToken = default)
    {
        ValidateRelationship(relationship);

        var (query, parameters) = CypherQueryBuilder.BuildCreateRelationshipQuery(relationship);
        await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
        _logger.LogDebug("Created Neo4j relationship: {Type} from {Source} to {Target}",
            relationship.Type, relationship.SourceNodeId, relationship.TargetNodeId);
    }

    public async Task CreateRelationshipsAsync(IEnumerable<GraphRelationship> relationships, CancellationToken cancellationToken = default)
    {
        var relList = relationships.ToList();
        if (relList.Count == 0) return;

        var parameters = new Dictionary<string, object>
        {
            ["relationships"] = relList.Select(r => new Dictionary<string, object>
            {
                ["sourceId"] = r.SourceNodeId,
                ["targetId"] = r.TargetNodeId,
                ["type"] = r.Type,
                ["props"] = r.Properties
            }).ToList()
        };

        var type = relList.First().Type;
        var query = "UNWIND $relationships AS rel " +
            "MATCH (a {id: rel.sourceId}), (b {id: rel.targetId}) " +
            $"CREATE (a)-[r:{type}]->(b) " +
            "SET r += rel.props";

        await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
        _logger.LogDebug("Created {Count} Neo4j relationships", relList.Count);
    }

    public async Task<IReadOnlyList<GraphRelationship>> GetRelationshipsAsync(string sourceNodeId, string targetNodeId, string? relationshipType = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);

        var (query, parameters) = CypherQueryBuilder.BuildGetRelationshipsQuery(sourceNodeId, targetNodeId, relationshipType);
        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Relationships;
    }

    public async Task<IReadOnlyList<GraphRelationship>> GetNodeRelationshipsAsync(string nodeId, RelationshipDirection direction = RelationshipDirection.Both, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var (query, parameters) = CypherQueryBuilder.BuildGetNodeRelationshipsQuery(nodeId, direction);
        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Relationships;
    }

    public async Task<bool> DeleteRelationshipAsync(string sourceNodeId, string targetNodeId, string? relationshipType = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);

        var (query, parameters) = CypherQueryBuilder.BuildDeleteRelationshipQuery(sourceNodeId, targetNodeId, relationshipType);
        await ExecuteWriteQueryAsync(query, parameters, cancellationToken);
        _logger.LogDebug("Deleted Neo4j relationship from {Source} to {Target}", sourceNodeId, targetNodeId);
        return true;
    }

    #endregion

    #region 查询操作

    public async Task<GraphQueryResult> ExecuteQueryAsync(string cypher, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cypher);

        return await ExecuteReadQueryAsync(cypher, parameters ?? new Dictionary<string, object>(), cancellationToken);
    }

    public async Task<GraphPath?> FindShortestPathAsync(string startNodeId, string endNodeId, int maxDepth = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(endNodeId);

        var query = """
            MATCH (start {id: $startId}), (end {id: $endId})
            MATCH path = shortestPath((start)-[*1..$maxDepth]-(end))
            RETURN path
            LIMIT 1
            """;

        var parameters = new Dictionary<string, object>
        {
            ["startId"] = startNodeId,
            ["endId"] = endNodeId,
            ["maxDepth"] = maxDepth
        };

        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Paths.FirstOrDefault();
    }

    public async Task<IReadOnlyList<GraphPath>> FindAllPathsAsync(string startNodeId, string endNodeId, int maxDepth = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(endNodeId);

        var (query, parameters) = CypherQueryBuilder.BuildAllPathsQuery(startNodeId, endNodeId, maxDepth);
        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Paths;
    }

    public async Task<IReadOnlyList<GraphNode>> GetNeighborsAsync(string nodeId, string? relationshipType = null, RelationshipDirection direction = RelationshipDirection.Both, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var (query, parameters) = CypherQueryBuilder.BuildGetNeighborsQuery(nodeId, relationshipType, direction);
        var result = await ExecuteReadQueryAsync(query, parameters, cancellationToken);
        return result.Nodes;
    }

    #endregion

    #region 图操作

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var query = CypherQueryBuilder.BuildClearQuery();
        await ExecuteWriteQueryAsync(query, new Dictionary<string, object>(), cancellationToken);
        _logger.LogInformation("Cleared Neo4j database");
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        var query = CypherQueryBuilder.BuildPingQuery();
        try
        {
            await ExecuteReadQueryAsync(query, new Dictionary<string, object>(), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neo4j ping failed");
            return false;
        }
    }

    #endregion

    #region 私有方法

    private async Task ExecuteWriteQueryAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(query, parameters);
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    private async Task<GraphQueryResult> ExecuteReadQueryAsync(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        try
        {
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, parameters);
                var records = await cursor.ToListAsync();

                return ParseRecords(records);
            });
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    private GraphQueryResult ParseRecords(List<IRecord> records)
    {
        var result = new GraphQueryResult();
        var nodeIds = new HashSet<string>();
        var relIds = new HashSet<string>();

        foreach (var record in records)
        {
            foreach (var value in record.Values)
            {
                ParseValue(value.Value, result, nodeIds, relIds);
            }
        }

        return result;
    }

    private void ParseValue(object? value, GraphQueryResult result, HashSet<string> nodeIds, HashSet<string> relIds)
    {
        switch (value)
        {
            case INode node:
                var graphNode = ConvertToGraphNode(node);
                if (nodeIds.Add(graphNode.Id))
                {
                    result.Nodes.Add(graphNode);
                }
                break;

            case IRelationship rel:
                var graphRel = ConvertToGraphRelationship(rel);
                if (relIds.Add(graphRel.Id!))
                {
                    result.Relationships.Add(graphRel);
                }
                break;

            case IPath path:
                result.Paths.Add(ConvertToGraphPath(path));
                break;

            case IEnumerable<object> list:
                foreach (var item in list)
                {
                    ParseValue(item, result, nodeIds, relIds);
                }
                break;
        }
    }

    private GraphNode ConvertToGraphNode(INode node)
    {
        var id = node.Properties.ContainsKey("id")
            ? node.Properties["id"].ToString()!
            : node.ElementId;

        return new GraphNode
        {
            Id = id,
            Label = node.Labels.FirstOrDefault() ?? "Node",
            Properties = node.Properties.ToDictionary(
                kv => kv.Key,
                kv => kv.Value)
        };
    }

    private GraphRelationship ConvertToGraphRelationship(IRelationship rel)
    {
        return new GraphRelationship
        {
            Id = rel.ElementId,
            Type = rel.Type,
            SourceNodeId = rel.StartNodeElementId,
            TargetNodeId = rel.EndNodeElementId,
            Properties = rel.Properties.ToDictionary(
                kv => kv.Key,
                kv => kv.Value)
        };
    }

    private GraphPath ConvertToGraphPath(IPath path)
    {
        var graphPath = new GraphPath
        {
            Nodes = path.Nodes.Select(ConvertToGraphNode).ToList(),
            Relationships = path.Relationships.Select(ConvertToGraphRelationship).ToList()
        };
        return graphPath;
    }

    private static void ValidateNode(GraphNode node, [CallerArgumentExpression(nameof(node))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(node, paramName);
        ArgumentException.ThrowIfNullOrWhiteSpace(node.Id, nameof(node.Id));
        ArgumentException.ThrowIfNullOrWhiteSpace(node.Label, nameof(node.Label));
    }

    private static void ValidateRelationship(GraphRelationship rel, [CallerArgumentExpression(nameof(rel))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(rel, paramName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rel.Type, nameof(rel.Type));
        ArgumentException.ThrowIfNullOrWhiteSpace(rel.SourceNodeId, nameof(rel.SourceNodeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(rel.TargetNodeId, nameof(rel.TargetNodeId));
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }

    #endregion
}
