namespace AI_RPG.Domain.Events;

/// <summary>
/// 领域事件接口
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// 领域事件基类
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
