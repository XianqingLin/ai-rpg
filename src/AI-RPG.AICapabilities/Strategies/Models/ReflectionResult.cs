namespace AI_RPG.AICapabilities.Strategies.Models;

/// <summary>
/// 反思结果
/// </summary>
public sealed class ReflectionResult
{
    /// <summary>
    /// 是否需要改进
    /// </summary>
    public required bool NeedsImprovement { get; init; }

    /// <summary>
    /// 反思内容
    /// </summary>
    public required string Reflection { get; init; }

    /// <summary>
    /// 建议改进点
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];

    /// <summary>
    /// 评分（1-10）
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// 改进后的内容
    /// </summary>
    public string? ImprovedContent { get; init; }
}

/// <summary>
/// 反思循环配置
/// </summary>
public sealed class ReflectionConfig
{
    /// <summary>
    /// 最大反思次数
    /// </summary>
    public int MaxReflections { get; set; } = 3;

    /// <summary>
    /// 最低可接受评分
    /// </summary>
    public int MinAcceptableScore { get; set; } = 7;

    /// <summary>
    /// 是否自动应用改进
    /// </summary>
    public bool AutoApplyImprovement { get; set; } = true;

    /// <summary>
    /// 反思提示模板
    /// </summary>
    public string ReflectionPromptTemplate { get; set; } = """
请对以下内容进行反思和评估：

原始内容：
{{content}}

请从以下几个方面评估：
1. 准确性：内容是否准确无误
2. 完整性：是否涵盖了所有必要信息
3. 连贯性：逻辑是否清晰连贯
4. 相关性：是否与问题紧密相关

请以JSON格式返回：
{
    "score": 1-10的评分,
    "needsImprovement": true/false,
    "reflection": "反思内容",
    "suggestions": ["改进建议1", "改进建议2"]
}
""";
}
