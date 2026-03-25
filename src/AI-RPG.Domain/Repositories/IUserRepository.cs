using AI_RPG.Domain.Entities;
using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Repositories;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户列表
    /// </summary>
    Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加用户
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除用户
    /// </summary>
    Task DeleteAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否已存在
    /// </summary>
    Task<bool> ExistsUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查邮箱是否已存在
    /// </summary>
    Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户是否存在
    /// </summary>
    Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);
}
