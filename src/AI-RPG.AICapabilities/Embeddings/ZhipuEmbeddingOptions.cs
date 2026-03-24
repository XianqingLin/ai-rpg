namespace AI_RPG.AICapabilities.Embeddings;

/// <summary>
/// 智谱Embedding配置选项
/// </summary>
public sealed class ZhipuEmbeddingOptions
{
    /// <summary>
    /// API密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 基础URL
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = "embedding-3";

    /// <summary>
    /// 向量维度（默认512）
    /// </summary>
    public int Dimensions { get; set; } = 512;

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
