using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Entities;

/// <summary>
/// 参与者抽象基类
/// </summary>
public abstract class Participant
{
    /// <summary>
    /// 参与者ID
    /// </summary>
    public ParticipantId Id { get; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 参与者类型
    /// </summary>
    public abstract ParticipantType Type { get; }

    /// <summary>
    /// 当前状态
    /// </summary>
    public ParticipantState State { get; private set; }

    protected Participant(ParticipantId id, string name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        State = ParticipantState.Active;
    }

    /// <summary>
    /// 激活参与者
    /// </summary>
    public void Activate()
    {
        State = ParticipantState.Active;
    }

    /// <summary>
    /// 停用参与者
    /// </summary>
    public void Deactivate()
    {
        State = ParticipantState.Inactive;
    }

    public override string ToString() => $"{Name} ({Type})";
}
