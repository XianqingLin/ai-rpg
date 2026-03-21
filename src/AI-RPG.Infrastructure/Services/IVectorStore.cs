using Qdrant.Client.Grpc;

namespace AI_RPG.Infrastructure.Services;

/// <summary>
/// 向量数据库接口
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// 创建集合
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="vectorSize">向量维度</param>
    /// <param name="distance">距离度量方式，默认为余弦相似度</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        Distance distance = Distance.Cosine,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除集合
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查集合是否存在
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有集合名称
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<string>> GetCollectionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 插入或更新向量点
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="point">向量点</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpsertAsync(
        string collectionName,
        VectorPoint point,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量插入或更新向量点
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="points">向量点列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpsertBatchAsync(
        string collectionName,
        IEnumerable<VectorPoint> points,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取向量点
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="id">点ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>向量点，不存在时返回null</returns>
    Task<VectorPoint?> GetAsync(
        string collectionName,
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量获取向量点
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="ids">点ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<VectorPoint>> GetBatchAsync(
        string collectionName,
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除向量点
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="id">点ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> DeleteAsync(
        string collectionName,
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除向量点
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="ids">点ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteBatchAsync(
        string collectionName,
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 相似度搜索
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="vector">查询向量</param>
    /// <param name="limit">返回结果数量</param>
    /// <param name="minScore">最小相似度分数（0-1）</param>
    /// <param name="filter">过滤条件</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        string collectionName,
        ReadOnlyMemory<float> vector,
        int limit = 10,
        float? minScore = null,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空集合中的所有数据
    /// </summary>
    /// <param name="collectionName">集合名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task ClearCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);
}
