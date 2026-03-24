using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Entities;

/// <summary>
/// 玩家实体
/// </summary>
public sealed class Player : Participant
{
    public override ParticipantType Type => ParticipantType.Player;

    /// <summary>
    /// 关联的用户ID（外部系统）
    /// </summary>
    public string UserId { get; }

    public Player(ParticipantId id, string name, string userId)
        : base(id, name)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }

    /// <summary>
    /// 创建新玩家
    /// </summary>
    public static Player Create(string name, string userId)
    {
        return new Player(ParticipantId.New(), name, userId);
    }
}
