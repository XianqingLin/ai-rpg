namespace AI_RPG.Domain.Entities;

/// <summary>
/// 参与者类型
/// </summary>
public enum ParticipantType
{
    /// <summary>
    /// 玩家
    /// </summary>
    Player,

    /// <summary>
    /// NPC
    /// </summary>
    NPC,

    /// <summary>
    /// 游戏主持人
    /// </summary>
    GameMaster
}

/// <summary>
/// 参与者状态
/// </summary>
public enum ParticipantState
{
    /// <summary>
    /// 活跃
    /// </summary>
    Active,

    /// <summary>
    /// 非活跃
    /// </summary>
    Inactive
}

/// <summary>
/// 会话状态
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// 准备中
    /// </summary>
    Preparing,

    /// <summary>
    /// 运行中
    /// </summary>
    Running,

    /// <summary>
    /// 暂停
    /// </summary>
    Paused,

    /// <summary>
    /// 已结束
    /// </summary>
    Ended
}
