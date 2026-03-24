using AI_RPG.Application.DTOs;

namespace AI_RPG.Application.Interfaces;

/// <summary>
/// 对话应用服务接口
/// </summary>
public interface IDialogueAppService
{
    /// <summary>
    /// 发送消息（玩家输入）
    /// </summary>
    Task<DialogueResponseDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取对话历史
    /// </summary>
    Task<IReadOnlyList<DialogueTurnDto>> GetHistoryAsync(GetHistoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 流式发送消息
    /// </summary>
    IAsyncEnumerable<string> StreamMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);
}
