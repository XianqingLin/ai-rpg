using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QdrantClientCore = Qdrant.Client.QdrantClient;
using QdrantException = Qdrant.Client.QdrantException;
using Qdrant.Client.Grpc;
using AI_RPG.Infrastructure.Services;
using static AI_RPG.Infrastructure.Implementations.VectorStore.QdrantFilterBuilder;
using static AI_RPG.Infrastructure.Implementations.VectorStore.QdrantConverters;

namespace AI_RPG.Infrastructure.Implementations.VectorStore;

/// <summary>
/// Qdrant向量数据库客户端实现
/// </summary>
public sealed class QdrantClient : IVectorStore, IDisposable
{
    private readonly QdrantClientCore _client;
    private readonly ILogger<QdrantClient> _logger;
    private readonly QdrantOptions _options;

    /// <summary>
    /// 初始化Qdrant客户端
    /// </summary>
    public QdrantClient(IOptions<QdrantOptions> options, ILogger<QdrantClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var uri = new Uri(_options.Uri);
        _client = new QdrantClientCore(
            host: uri.Host,
            port: uri.Port,
            apiKey: _options.ApiKey,
            https: uri.Scheme == "https");

        _logger.LogInformation("Qdrant client initialized: {Uri}", _options.Uri);
    }

    public async Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        Distance distance = Distance.Cosine,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(vectorSize, 0, nameof(vectorSize));

        try
        {
            await _client.CreateCollectionAsync(
                collectionName: collectionName,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)vectorSize,
                    Distance = distance
                },
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created Qdrant collection: {CollectionName}, VectorSize: {VectorSize}, Distance: {Distance}",
                collectionName, vectorSize, distance);
        }
        catch (QdrantException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Qdrant collection already exists: {CollectionName}", collectionName);
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.", ex);
        }
    }

    public async Task<bool> DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);

        try
        {
            await _client.DeleteCollectionAsync(
                collectionName: collectionName,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted Qdrant collection: {CollectionName}", collectionName);
            return true;
        }
        catch (QdrantException ex) when (ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Qdrant collection not found for deletion: {CollectionName}", collectionName);
            return false;
        }
    }

    public async Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);

        var collections = await _client.ListCollectionsAsync(cancellationToken);
        return collections.Contains(collectionName);
    }

    public async Task<IReadOnlyList<string>> GetCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = await _client.ListCollectionsAsync(cancellationToken);
        return collections.ToList();
    }

    public async Task UpsertAsync(
        string collectionName,
        VectorPoint point,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentNullException.ThrowIfNull(point);

        await UpsertBatchAsync(collectionName, [point], cancellationToken);
    }

    public async Task UpsertBatchAsync(
        string collectionName,
        IEnumerable<VectorPoint> points,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentNullException.ThrowIfNull(points);

        var pointList = points.ToList();
        if (pointList.Count == 0)
        {
            return;
        }

        var qdrantPoints = pointList.Select(p => new PointStruct
        {
            Id = new PointId { Uuid = p.Id },
            Vectors = p.Vector.ToArray(),
            Payload = { ConvertPayload(p.Payload) },
        }).ToList();

        await _client.UpsertAsync(
            collectionName: collectionName,
            points: qdrantPoints,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Upserted {Count} points to Qdrant collection: {CollectionName}",
            pointList.Count, collectionName);
    }

    public async Task<VectorPoint?> GetAsync(
        string collectionName,
        string id,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var results = await GetBatchAsync(collectionName, [id], cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<VectorPoint>> GetBatchAsync(
        string collectionName,
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        var pointIds = idList.Select(id => new PointId { Uuid = id }).ToList();

        var points = await _client.RetrieveAsync(
            collectionName: collectionName,
            ids: pointIds,
            withVectors: true,
            cancellationToken: cancellationToken);

        return points.Select(ConvertToVectorPoint).ToList();
    }

    public async Task<bool> DeleteAsync(
        string collectionName,
        string id,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await _client.DeleteAsync(
            collectionName: collectionName,
            ids: [new PointId { Uuid = id }],
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Deleted point {Id} from Qdrant collection: {CollectionName}",
            id, collectionName);

        return true;
    }

    public async Task DeleteBatchAsync(
        string collectionName,
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return;
        }

        var pointIds = idList.Select(id => new PointId { Uuid = id }).ToList();

        await _client.DeleteAsync(
            collectionName: collectionName,
            ids: pointIds,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Deleted {Count} points from Qdrant collection: {CollectionName}",
            idList.Count, collectionName);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string collectionName,
        ReadOnlyMemory<float> vector,
        int limit = 10,
        float? minScore = null,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, 0, nameof(limit));

        var results = await _client.SearchAsync(
            collectionName: collectionName,
            vector: vector.ToArray(),
            limit: (ulong)limit,
            scoreThreshold: minScore,
            filter: filter is not null ? BuildFilter(filter) : null,
            cancellationToken: cancellationToken);

        return results.Select(ConvertToSearchResult).ToList();
    }

    public async Task ClearCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);

        try
        {
            await _client.DeleteCollectionAsync(collectionName, cancellationToken: cancellationToken);

            await _client.CreateCollectionAsync(
                collectionName: collectionName,
                vectorsConfig: new VectorParams
                {
                    Size = 1536,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken);

            _logger.LogInformation("Cleared Qdrant collection: {CollectionName}", collectionName);
        }
        catch (QdrantException ex) when (ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Qdrant collection not found for clearing: {CollectionName}", collectionName);
        }
    }

    private static void ValidateCollectionName(string collectionName, [CallerArgumentExpression(nameof(collectionName))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName, paramName);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
