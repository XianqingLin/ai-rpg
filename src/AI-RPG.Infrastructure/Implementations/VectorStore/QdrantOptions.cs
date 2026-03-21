namespace AI_RPG.Infrastructure.Implementations.VectorStore;

/// <summary>
/// Qdrant配置选项
/// </summary>
public sealed class QdrantOptions
{
    /// <summary>
    /// 连接URI
    /// </summary>
    public string Uri { get; set; } = "http://localhost:6333";

    /// <summary>
    /// API密钥（可选）
    /// </summary>
    public string? ApiKey { get; set; }
}
