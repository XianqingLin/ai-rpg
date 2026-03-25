using AI_RPG.Domain.Entities;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.ValueObjects;
using AI_RPG.Infrastructure.Data;
using AI_RPG.Infrastructure.Data.Sql;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AI_RPG.Infrastructure.Repositories;

/// <summary>
/// 用户仓储 PostgreSQL 实现
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value?.ConnectionString 
            ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.GetById, connection);
        command.Parameters.AddWithValue("@id", id.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapToUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.GetByUsername, connection);
        command.Parameters.AddWithValue("@username", username);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapToUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.GetByEmail, connection);
        command.Parameters.AddWithValue("@email", email.ToLowerInvariant());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapToUser(reader);
        }

        return null;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = new List<User>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.GetAll, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(MapToUser(reader));
        }

        return users.AsReadOnly();
    }

    public async Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = new List<User>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.GetActiveUsers, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(MapToUser(reader));
        }

        return users.AsReadOnly();
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.Insert, connection);
        AddUserParameters(command, user);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.Update, connection);
        AddUserParameters(command, user);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.Delete, connection);
        command.Parameters.AddWithValue("@id", id.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ExistsUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.ExistsUsername, connection);
        command.Parameters.AddWithValue("@username", username);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    public async Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.ExistsEmail, connection);
        command.Parameters.AddWithValue("@email", email.ToLowerInvariant());

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(UserSql.Exists, connection);
        command.Parameters.AddWithValue("@id", id.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    /// <summary>
    /// 将数据库记录映射为用户实体
    /// </summary>
    private static User MapToUser(NpgsqlDataReader reader)
    {
        var id = new UserId(reader.GetString(0));
        var username = reader.GetString(1);
        var email = reader.GetString(2);
        var passwordHash = reader.GetString(3);
        var displayName = reader.IsDBNull(4) ? null : reader.GetString(4);
        var avatarUrl = reader.IsDBNull(5) ? null : reader.GetString(5);
        var isActive = reader.GetBoolean(6);
        var createdAt = reader.GetDateTime(7);
        var updatedAt = reader.GetDateTime(8);
        var lastLoginAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);

        return User.Reconstitute(
            id, username, email, passwordHash, displayName, avatarUrl,
            isActive, createdAt, updatedAt, lastLoginAt);
    }

    /// <summary>
    /// 添加用户参数到命令
    /// </summary>
    private static void AddUserParameters(NpgsqlCommand command, User user)
    {
        command.Parameters.AddWithValue("@id", user.Id.Value);
        command.Parameters.AddWithValue("@username", user.Username);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@displayName", user.DisplayName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@avatarUrl", user.AvatarUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@isActive", user.IsActive);
        command.Parameters.AddWithValue("@createdAt", user.CreatedAt);
        command.Parameters.AddWithValue("@updatedAt", user.UpdatedAt);
        command.Parameters.AddWithValue("@lastLoginAt", user.LastLoginAt ?? (object)DBNull.Value);
    }
}
