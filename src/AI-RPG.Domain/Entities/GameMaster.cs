using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Entities;

/// <summary>
/// 游戏主持人实体
/// </summary>
public sealed class GameMaster : Participant
{
    public override ParticipantType Type => ParticipantType.GameMaster;

    /// <summary>
    /// 默认主持人名称
    /// </summary>
    public const string DefaultName = "主持人";

    public GameMaster(ParticipantId id, string? name = null)
        : base(id, name ?? DefaultName)
    {
    }

    /// <summary>
    /// 创建默认主持人
    /// </summary>
    public static GameMaster CreateDefault()
    {
        return new GameMaster(ParticipantId.New());
    }
}
