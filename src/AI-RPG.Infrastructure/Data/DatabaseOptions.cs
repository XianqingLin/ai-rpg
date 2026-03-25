namespace AI_RPG.Infrastructure.Data;

/// <summary>
/// 数据库配置选项
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// PostgreSQL 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
