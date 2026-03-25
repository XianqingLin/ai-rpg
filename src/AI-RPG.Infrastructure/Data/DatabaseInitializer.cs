using Microsoft.Extensions.Logging;
using Npgsql;

namespace AI_RPG.Infrastructure.Data;

/// <summary>
/// 数据库初始化器
/// </summary>
public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer>? _logger;

    public DatabaseInitializer(string connectionString, ILogger<DatabaseInitializer>? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    /// <summary>
    /// 初始化数据库（创建数据库和表结构）
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Initializing database...");

        // 先创建数据库（如果不存在）
        await CreateDatabaseIfNotExistsAsync(cancellationToken);

        // 然后创建表
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await CreateUsersTableAsync(connection, cancellationToken);

        _logger?.LogInformation("Database initialized successfully.");
    }

    /// <summary>
    /// 创建数据库（如果不存在）
    /// </summary>
    private async Task CreateDatabaseIfNotExistsAsync(CancellationToken cancellationToken)
    {
        // 解析连接字符串，构建连接到 postgres 系统数据库的连接字符串
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres"; // 连接到系统数据库

        try
        {
            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // 检查数据库是否存在
            var checkDbSql = "SELECT 1 FROM pg_database WHERE datname = @dbname";
            await using var checkCmd = new NpgsqlCommand(checkDbSql, connection);
            checkCmd.Parameters.AddWithValue("@dbname", databaseName);
            var exists = await checkCmd.ExecuteScalarAsync(cancellationToken) != null;

            if (!exists)
            {
                _logger?.LogInformation("Creating database: {DatabaseName}", databaseName);
                
                // 创建数据库
                var createDbSql = $"CREATE DATABASE \"{databaseName}\"";
                await using var createCmd = new NpgsqlCommand(createDbSql, connection);
                await createCmd.ExecuteNonQueryAsync(cancellationToken);
                
                _logger?.LogInformation("Database created: {DatabaseName}", databaseName);
            }
            else
            {
                _logger?.LogInformation("Database already exists: {DatabaseName}", databaseName);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create database: {DatabaseName}", databaseName);
            throw;
        }
    }

    /// <summary>
    /// 创建用户表
    /// </summary>
    private async Task CreateUsersTableAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS users (
                id VARCHAR(32) PRIMARY KEY,
                username VARCHAR(50) NOT NULL,
                email VARCHAR(255) NOT NULL,
                password_hash VARCHAR(255) NOT NULL,
                display_name VARCHAR(100),
                avatar_url TEXT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                last_login_at TIMESTAMP WITH TIME ZONE
            );

            -- 创建唯一索引
            CREATE UNIQUE INDEX IF NOT EXISTS idx_users_username ON users (LOWER(username));
            CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users (LOWER(email));

            -- 创建普通索引
            CREATE INDEX IF NOT EXISTS idx_users_is_active ON users (is_active);
            CREATE INDEX IF NOT EXISTS idx_users_created_at ON users (created_at DESC);
        ";

        await using var command = new NpgsqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger?.LogInformation("Users table created or already exists.");
    }
}
