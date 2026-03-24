using AI_RPG.Domain.Entities;
using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Services;

/// <summary>
/// 对话处理结果
/// </summary>
public sealed class DialogueResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 响应内容
    /// </summary>
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// 响应类型
    /// </summary>
    public DialogueType Type { get; init; }

    /// <summary>
    /// 新创建的对话回合
    /// </summary>
    public IReadOnlyList<DialogueTurn> NewTurns { get; init; } = new List<DialogueTurn>();

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// 对话服务接口
/// </summary>
public interface IDialogueService
{
    /// <summary>
    /// 处理玩家输入
    /// </summary>
    Task<DialogueResult> ProcessPlayerInputAsync(
        SessionId sessionId,
        ParticipantId playerId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成NPC响应
    /// </summary>
    Task<DialogueResult> GenerateNPCResponseAsync(
        SessionId sessionId,
        ParticipantId npcId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成主持人叙述
    /// </summary>
    Task<DialogueResult> GenerateNarrationAsync(
        SessionId sessionId,
        string context,
        CancellationToken cancellationToken = default);
}
