namespace AI_RPG.Domain.ValueObjects;

/// <summary>
/// 对话类型
/// </summary>
public enum DialogueType
{
    /// <summary>
    /// 说话
    /// </summary>
    Speech,

    /// <summary>
    /// 行动
    /// </summary>
    Action,

    /// <summary>
    /// 叙述
    /// </summary>
    Narration,

    /// <summary>
    /// 系统消息
    /// </summary>
    System
}

/// <summary>
/// 对话回合值对象
/// </summary>
public sealed class DialogueTurn : ValueObject
{
    /// <summary>
    /// 回合序号
    /// </summary>
    public int TurnNumber { get; }

    /// <summary>
    /// 说话者ID
    /// </summary>
    public ParticipantId SpeakerId { get; }

    /// <summary>
    /// 说话者名称
    /// </summary>
    public string SpeakerName { get; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// 对话类型
    /// </summary>
    public DialogueType Type { get; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; }

    public DialogueTurn(
        int turnNumber,
        ParticipantId speakerId,
        string speakerName,
        string content,
        DialogueType type,
        DateTime? timestamp = null)
    {
        TurnNumber = turnNumber;
        SpeakerId = speakerId ?? throw new ArgumentNullException(nameof(speakerId));
        SpeakerName = speakerName ?? throw new ArgumentNullException(nameof(speakerName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Type = type;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TurnNumber;
        yield return SpeakerId;
        yield return Content;
        yield return Type;
        yield return Timestamp;
    }

    public override string ToString() => $"[{SpeakerName}] {Content}";
}
