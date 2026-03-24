using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Events;

/// <summary>
/// 对话已发生事件
/// </summary>
public sealed class DialogueSpoken : DomainEvent
{
    public SessionId SessionId { get; }
    public int TurnNumber { get; }
    public ParticipantId SpeakerId { get; }
    public string SpeakerName { get; }
    public string Content { get; }
    public DialogueType Type { get; }

    public DialogueSpoken(
        SessionId sessionId,
        int turnNumber,
        ParticipantId speakerId,
        string speakerName,
        string content,
        DialogueType type)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        TurnNumber = turnNumber;
        SpeakerId = speakerId ?? throw new ArgumentNullException(nameof(speakerId));
        SpeakerName = speakerName ?? throw new ArgumentNullException(nameof(speakerName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Type = type;
    }
}
