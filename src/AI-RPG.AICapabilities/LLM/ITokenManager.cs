namespace AI_RPG.AICapabilities.LLM;

/// <summary>
/// Token使用统计
/// </summary>
public sealed class TokenUsage
{
    /// <summary>
    /// 输入Token数
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// 输出Token数
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// 总Token数
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 操作类型
    /// </summary>
    public string? OperationType { get; set; }
}

/// <summary>
/// Token限制配置
/// </summary>
public sealed class TokenLimitConfig
{
    /// <summary>
    /// 每分钟最大Token数
    /// </summary>
    public int MaxTokensPerMinute { get; set; } = 100000;

    /// <summary>
    /// 每小时最大Token数
    /// </summary>
    public int MaxTokensPerHour { get; set; } = 1000000;

    /// <summary>
    /// 每天最大Token数
    /// </summary>
    public int MaxTokensPerDay { get; set; } = 10000000;

    /// <summary>
    /// 单次请求最大Token数
    /// </summary>
    public int MaxTokensPerRequest { get; set; } = 4096;
}

/// <summary>
/// Token管理器接口
/// </summary>
public interface ITokenManager
{
    /// <summary>
    /// 记录Token使用
    /// </summary>
    void RecordUsage(TokenUsage usage);

    /// <summary>
    /// 检查是否超出限制
    /// </summary>
    bool IsLimitExceeded(string? model = null);

    /// <summary>
    /// 获取当前使用统计
    /// </summary>
    TokenUsage GetCurrentUsage(string? model = null);

    /// <summary>
    /// 获取历史使用记录
    /// </summary>
    IReadOnlyList<TokenUsage> GetUsageHistory(TimeSpan period);

    /// <summary>
    /// 预估Token数
    /// </summary>
    int EstimateTokens(string text);

    /// <summary>
    /// 配置限制
    /// </summary>
    void ConfigureLimit(TokenLimitConfig config);

    /// <summary>
    /// 重置统计
    /// </summary>
    void ResetStatistics();
}

/// <summary>
/// Token管理器实现
/// </summary>
public sealed class TokenManager : ITokenManager
{
    private readonly List<TokenUsage> _usageHistory = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private TokenLimitConfig _config = new();

    // 简单估算：中文约1.5个token/字，英文约0.25个token/字
    public int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int chineseChars = text.Count(c => c >= 0x4E00 && c <= 0x9FFF);
        int otherChars = text.Length - chineseChars;

        return (int)(chineseChars * 1.5 + otherChars * 0.25);
    }

    public void RecordUsage(TokenUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage);

        _lock.EnterWriteLock();
        try
        {
            _usageHistory.Add(usage);
            
            // 清理过期数据（保留7天）
            var cutoff = DateTime.UtcNow.AddDays(-7);
            _usageHistory.RemoveAll(u => u.Timestamp < cutoff);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool IsLimitExceeded(string? model = null)
    {
        _lock.EnterReadLock();
        try
        {
            var now = DateTime.UtcNow;
            var recentUsages = _usageHistory.Where(u => 
                (model == null || u.Model == model) &&
                u.Timestamp > now.AddMinutes(-1));

            var minuteTotal = recentUsages.Sum(u => u.TotalTokens);
            if (minuteTotal >= _config.MaxTokensPerMinute)
                return true;

            var hourTotal = _usageHistory
                .Where(u => (model == null || u.Model == model) && u.Timestamp > now.AddHours(-1))
                .Sum(u => u.TotalTokens);
            if (hourTotal >= _config.MaxTokensPerHour)
                return true;

            var dayTotal = _usageHistory
                .Where(u => (model == null || u.Model == model) && u.Timestamp > now.AddDays(-1))
                .Sum(u => u.TotalTokens);
            if (dayTotal >= _config.MaxTokensPerDay)
                return true;

            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TokenUsage GetCurrentUsage(string? model = null)
    {
        _lock.EnterReadLock();
        try
        {
            var now = DateTime.UtcNow;
            var recentUsages = _usageHistory.Where(u => 
                (model == null || u.Model == model) &&
                u.Timestamp > now.AddMinutes(-1)).ToList();

            return new TokenUsage
            {
                InputTokens = recentUsages.Sum(u => u.InputTokens),
                OutputTokens = recentUsages.Sum(u => u.OutputTokens),
                Model = model,
                Timestamp = now
            };
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<TokenUsage> GetUsageHistory(TimeSpan period)
    {
        _lock.EnterReadLock();
        try
        {
            var cutoff = DateTime.UtcNow - period;
            return _usageHistory.Where(u => u.Timestamp >= cutoff).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void ConfigureLimit(TokenLimitConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    public void ResetStatistics()
    {
        _lock.EnterWriteLock();
        try
        {
            _usageHistory.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
