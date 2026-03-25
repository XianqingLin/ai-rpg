namespace AI_RPG.Infrastructure.Data.Sql;

/// <summary>
/// 用户表 SQL 语句
/// </summary>
public static class UserSql
{
    /// <summary>
    /// 根据ID查询用户
    /// </summary>
    public const string GetById = @"
        SELECT id, username, email, password_hash, display_name, avatar_url, 
               is_active, created_at, updated_at, last_login_at
        FROM users 
        WHERE id = @id";

    /// <summary>
    /// 根据用户名查询用户
    /// </summary>
    public const string GetByUsername = @"
        SELECT id, username, email, password_hash, display_name, avatar_url, 
               is_active, created_at, updated_at, last_login_at
        FROM users 
        WHERE LOWER(username) = LOWER(@username)";

    /// <summary>
    /// 根据邮箱查询用户
    /// </summary>
    public const string GetByEmail = @"
        SELECT id, username, email, password_hash, display_name, avatar_url, 
               is_active, created_at, updated_at, last_login_at
        FROM users 
        WHERE LOWER(email) = LOWER(@email)";

    /// <summary>
    /// 查询所有用户
    /// </summary>
    public const string GetAll = @"
        SELECT id, username, email, password_hash, display_name, avatar_url, 
               is_active, created_at, updated_at, last_login_at
        FROM users 
        ORDER BY created_at DESC";

    /// <summary>
    /// 查询活跃用户
    /// </summary>
    public const string GetActiveUsers = @"
        SELECT id, username, email, password_hash, display_name, avatar_url, 
               is_active, created_at, updated_at, last_login_at
        FROM users 
        WHERE is_active = true
        ORDER BY created_at DESC";

    /// <summary>
    /// 插入用户
    /// </summary>
    public const string Insert = @"
        INSERT INTO users (id, username, email, password_hash, display_name, avatar_url, 
                          is_active, created_at, updated_at, last_login_at)
        VALUES (@id, @username, @email, @passwordHash, @displayName, @avatarUrl, 
                @isActive, @createdAt, @updatedAt, @lastLoginAt)";

    /// <summary>
    /// 更新用户
    /// </summary>
    public const string Update = @"
        UPDATE users 
        SET username = @username,
            email = @email,
            password_hash = @passwordHash,
            display_name = @displayName,
            avatar_url = @avatarUrl,
            is_active = @isActive,
            updated_at = @updatedAt,
            last_login_at = @lastLoginAt
        WHERE id = @id";

    /// <summary>
    /// 删除用户
    /// </summary>
    public const string Delete = "DELETE FROM users WHERE id = @id";

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    public const string ExistsUsername = "SELECT EXISTS(SELECT 1 FROM users WHERE LOWER(username) = LOWER(@username))";

    /// <summary>
    /// 检查邮箱是否存在
    /// </summary>
    public const string ExistsEmail = "SELECT EXISTS(SELECT 1 FROM users WHERE LOWER(email) = LOWER(@email))";

    /// <summary>
    /// 检查用户是否存在
    /// </summary>
    public const string Exists = "SELECT EXISTS(SELECT 1 FROM users WHERE id = @id)";
}
