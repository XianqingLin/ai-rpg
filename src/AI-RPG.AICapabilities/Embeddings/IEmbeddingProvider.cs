namespace AI_RPG.AICapabilities.Embeddings;

/// <summary>
/// 文本嵌入提供商接口
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// 提供商名称
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 向量维度
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// 生成单个文本的嵌入向量
    /// </summary>
    /// <param name="text">输入文本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量生成嵌入向量
    /// </summary>
    /// <param name="texts">输入文本列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>嵌入向量列表</returns>
    Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}

/// <summary>
/// 嵌入结果
/// </summary>
public sealed class EmbeddingResult
{
    /// <summary>
    /// 嵌入向量
    /// </summary>
    public required float[] Vector { get; init; }

    /// <summary>
    /// 使用的Token数
    /// </summary>
    public int TokenCount { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }
}
