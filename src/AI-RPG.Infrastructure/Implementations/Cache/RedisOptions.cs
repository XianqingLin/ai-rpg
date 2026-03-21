namespace AI_RPG.Infrastructure.Implementations.Cache;

/// <summary>
/// Redis 配置选项
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// 用户名（Redis 6.0+ ACL 支持）
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 默认数据库索引（0-15）
    /// </summary>
    public int DefaultDatabase { get; set; } = 0;

    /// <summary>
    /// 连接超时（毫秒）
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// 同步操作超时（毫秒）
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// 异步操作超时（毫秒）
    /// </summary>
    public int AsyncTimeout { get; set; } = 5000;

    /// <summary>
    /// 是否启用 SSL
    /// </summary>
    public bool Ssl { get; set; } = false;

    /// <summary>
    /// SSL 主机名
    /// </summary>
    public string? SslHost { get; set; }

    /// <summary>
    /// 连接重试次数
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// 连接池大小
    /// </summary>
    public int PoolSize { get; set; } = 10;

    /// <summary>
    /// 键前缀
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// 默认过期时间
    /// </summary>
    public TimeSpan? DefaultExpiration { get; set; }

    /// <summary>
    /// 是否启用压缩
    /// </summary>
    public bool EnableCompression { get; set; } = false;
}
