namespace AI_RPG.AICapabilities.LLM;

/// <summary>
/// Kimi AI 配置选项
/// </summary>
public sealed class KimiOptions
{
    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 模型名称，默认为 kimi-k2-5
    /// </summary>
    public string ModelName { get; set; } = "kimi-k2-5";

    /// <summary>
    /// API 基础地址，默认为 Moonshot AI 官方地址
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.moonshot.cn/v1";

    /// <summary>
    /// 温度参数 (0-2)，kimi-k2-5 使用确定性值 1
    /// </summary>
    public float Temperature { get; set; } = 1.0f;

    /// <summary>
    /// 最大 Token 数
    /// </summary>
    public int? MaxTokens { get; set; } = 4096;
}
