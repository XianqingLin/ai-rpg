namespace AI_RPG.Application.DTOs;

/// <summary>
/// 对话回合 DTO
/// </summary>
public sealed class DialogueTurnDto
{
    public int TurnNumber { get; set; }
    public string SpeakerId { get; set; } = string.Empty;
    public string SpeakerName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 发送消息请求
/// </summary>
public sealed class SendMessageRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 对话响应 DTO
/// </summary>
public sealed class DialogueResponseDto
{
    public bool Success { get; set; }
    public string SpeakerId { get; set; } = string.Empty;
    public string SpeakerName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 获取历史请求
/// </summary>
public sealed class GetHistoryRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int Count { get; set; } = 20;
}
