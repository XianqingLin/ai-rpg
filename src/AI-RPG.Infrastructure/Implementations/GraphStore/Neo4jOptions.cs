namespace AI_RPG.Infrastructure.Implementations.GraphStore;

/// <summary>
/// Neo4j 配置选项
/// </summary>
public sealed class Neo4jOptions
{
    /// <summary>
    /// 连接URI（例如：bolt://localhost:7687）
    /// </summary>
    public string Uri { get; set; } = "bolt://localhost:7687";

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = "neo4j";

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 数据库名称（可选，Neo4j 4.0+ 支持多数据库）
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// 连接超时（秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// 最大连接池大小
    /// </summary>
    public int MaxConnectionPoolSize { get; set; } = 100;

    /// <summary>
    /// 是否启用加密
    /// </summary>
    public bool Encrypted { get; set; } = false;
}
