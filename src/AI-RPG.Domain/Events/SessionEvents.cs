using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Events;

/// <summary>
/// 会话已开始事件
/// </summary>
public sealed class SessionStarted : DomainEvent
{
    public SessionId SessionId { get; }
    public DateTime StartedAt { get; }

    public SessionStarted(SessionId sessionId)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        StartedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// 会话已结束事件
/// </summary>
public sealed class SessionEnded : DomainEvent
{
    public SessionId SessionId { get; }
    public DateTime EndedAt { get; }
    public string Reason { get; }

    public SessionEnded(SessionId sessionId, string reason)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        EndedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// 玩家加入事件
/// </summary>
public sealed class PlayerJoined : DomainEvent
{
    public SessionId SessionId { get; }
    public ParticipantId PlayerId { get; }
    public string PlayerName { get; }

    public PlayerJoined(SessionId sessionId, ParticipantId playerId, string playerName)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        PlayerName = playerName ?? throw new ArgumentNullException(nameof(playerName));
    }
}

/// <summary>
/// 玩家离开事件
/// </summary>
public sealed class PlayerLeft : DomainEvent
{
    public SessionId SessionId { get; }
    public ParticipantId PlayerId { get; }
    public string Reason { get; }

    public PlayerLeft(SessionId sessionId, ParticipantId playerId, string reason)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }
}

/// <summary>
/// NPC进入场景事件
/// </summary>
public sealed class NPCEnteredScene : DomainEvent
{
    public SessionId SessionId { get; }
    public ParticipantId NPCId { get; }
    public string NPCName { get; }

    public NPCEnteredScene(SessionId sessionId, ParticipantId npcId, string npcName)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        NPCId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        NPCName = npcName ?? throw new ArgumentNullException(nameof(npcName));
    }
}

/// <summary>
/// NPC离开场景事件
/// </summary>
public sealed class NPCLeftScene : DomainEvent
{
    public SessionId SessionId { get; }
    public ParticipantId NPCId { get; }

    public NPCLeftScene(SessionId sessionId, ParticipantId npcId)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        NPCId = npcId ?? throw new ArgumentNullException(nameof(npcId));
    }
}
