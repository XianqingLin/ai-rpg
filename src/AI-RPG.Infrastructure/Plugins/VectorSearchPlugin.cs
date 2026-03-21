using AI_RPG.Infrastructure.Services;

namespace AI_RPG.Infrastructure.Plugins;

/// <summary>
/// 向量搜索插件 - 为 Semantic Kernel 提供向量检索能力
/// </summary>
public class VectorSearchPlugin
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingProvider _embeddingProvider;

    public VectorSearchPlugin(IVectorStore vectorStore, IEmbeddingProvider embeddingProvider)
    {
        _vectorStore = vectorStore;
        _embeddingProvider = embeddingProvider;
    }

    /// <summary>
    /// 搜索相似内容
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchSimilarAsync(
        string collectionName,
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        // 1. 生成查询向量
        var embedding = await _embeddingProvider.GenerateEmbeddingAsync(query, cancellationToken);

        // 2. 执行向量搜索
        var results = await _vectorStore.SearchAsync(
            collectionName: collectionName,
            vector: embedding,
            limit: topK,
            cancellationToken: cancellationToken);

        return results;
    }

    /// <summary>
    /// 带过滤条件的向量搜索
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchWithFilterAsync(
        string collectionName,
        string query,
        Dictionary<string, object> filter,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingProvider.GenerateEmbeddingAsync(query, cancellationToken);

        var results = await _vectorStore.SearchAsync(
            collectionName: collectionName,
            vector: embedding,
            limit: topK,
            filter: filter,
            cancellationToken: cancellationToken);

        return results;
    }

    /// <summary>
    /// 根据ID获取文档
    /// </summary>
    public async Task<VectorPoint?> GetDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        return await _vectorStore.GetAsync(collectionName, documentId, cancellationToken);
    }
}
