using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Entities;

/// <summary>
/// NPC实体
/// </summary>
public sealed class NPC : Participant
{
    public override ParticipantType Type => ParticipantType.NPC;

    /// <summary>
    /// NPC角色设定
    /// </summary>
    public NPCProfile Profile { get; }

    /// <summary>
    /// 是否在当前场景
    /// </summary>
    public bool IsPresent { get; private set; }

    public NPC(ParticipantId id, string name, NPCProfile profile)
        : base(id, name)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        IsPresent = true;
    }

    /// <summary>
    /// 创建新NPC
    /// </summary>
    public static NPC Create(string name, NPCProfile profile)
    {
        return new NPC(ParticipantId.New(), name, profile);
    }

    /// <summary>
    /// 进入场景
    /// </summary>
    public void EnterScene()
    {
        IsPresent = true;
    }

    /// <summary>
    /// 离开场景
    /// </summary>
    public void LeaveScene()
    {
        IsPresent = false;
    }
}
