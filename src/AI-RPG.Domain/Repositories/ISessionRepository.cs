using AI_RPG.Domain.Entities;
using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Repositories;

/// <summary>
/// 会话仓储接口
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// 根据ID获取会话
    /// </summary>
    Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的活跃会话列表
    /// </summary>
    Task<IReadOnlyList<Session>> GetActiveByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有运行中的会话
    /// </summary>
    Task<IReadOnlyList<Session>> GetRunningSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加会话
    /// </summary>
    Task AddAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新会话
    /// </summary>
    Task UpdateAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除会话
    /// </summary>
    Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查会话是否存在
    /// </summary>
    Task<bool> ExistsAsync(SessionId id, CancellationToken cancellationToken = default);
}
