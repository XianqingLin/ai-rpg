using AI_RPG.Domain.Entities;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Infrastructure.Repositories;

/// <summary>
/// 内存会话仓储实现 - 用于快速验证和测试
/// </summary>
public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly Dictionary<string, Session> _sessions = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            _sessions.TryGetValue(id.Value, out var session);
            return Task.FromResult(session);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task<IReadOnlyList<Session>> GetActiveByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var sessions = _sessions.Values
                .Where(s => s.Status != SessionStatus.Ended)
                .Where(s => s.GetPlayers().Any(p => p.UserId == userId))
                .ToList()
                .AsReadOnly();
            return Task.FromResult<IReadOnlyList<Session>>(sessions);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task<IReadOnlyList<Session>> GetRunningSessionsAsync(CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var sessions = _sessions.Values
                .Where(s => s.Status == SessionStatus.Running)
                .ToList()
                .AsReadOnly();
            return Task.FromResult<IReadOnlyList<Session>>(sessions);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        _lock.EnterWriteLock();
        try
        {
            if (_sessions.ContainsKey(session.Id.Value))
            {
                throw new InvalidOperationException($"Session with ID {session.Id} already exists");
            }

            _sessions[session.Id.Value] = session;
            return Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        _lock.EnterWriteLock();
        try
        {
            if (!_sessions.ContainsKey(session.Id.Value))
            {
                throw new InvalidOperationException($"Session with ID {session.Id} does not exist");
            }

            _sessions[session.Id.Value] = session;
            return Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Task DeleteAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();
        try
        {
            _sessions.Remove(id.Value);
            return Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Task<bool> ExistsAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            return Task.FromResult(_sessions.ContainsKey(id.Value));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 获取所有会话（用于调试）
    /// </summary>
    public IReadOnlyList<Session> GetAllSessions()
    {
        _lock.EnterReadLock();
        try
        {
            return _sessions.Values.ToList().AsReadOnly();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 清空所有会话（用于测试）
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _sessions.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
