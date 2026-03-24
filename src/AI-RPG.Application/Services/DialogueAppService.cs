using System.Runtime.CompilerServices;
using System.Text;
using AI_RPG.Application.DTOs;
using AI_RPG.Application.Interfaces;
using AI_RPG.Application.Mappings;
using AI_RPG.Domain.Entities;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.Services;
using AI_RPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AI_RPG.Application.Services;

/// <summary>
/// 对话应用服务实现
/// </summary>
public sealed class DialogueAppService : IDialogueAppService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IDialogueService _dialogueService;
    private readonly ILogger<DialogueAppService> _logger;

    public DialogueAppService(
        ISessionRepository sessionRepository,
        IDialogueService dialogueService,
        ILogger<DialogueAppService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _dialogueService = dialogueService ?? throw new ArgumentNullException(nameof(dialogueService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DialogueResponseDto> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Processing message in session {SessionId} from player {PlayerId}",
            request.SessionId, request.PlayerId);

        // 1. 获取会话
        var session = await _sessionRepository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {request.SessionId} not found");

        // 2. 验证会话状态
        if (session.Status != SessionStatus.Running)
        {
            return new DialogueResponseDto
            {
                Success = false,
                ErrorMessage = $"Session is not running. Current status: {session.Status}"
            };
        }

        // 3. 调用领域服务处理输入
        var result = await _dialogueService.ProcessPlayerInputAsync(
            session.Id,
            new ParticipantId(request.PlayerId),
            request.Message,
            cancellationToken);

        // 4. 保存会话（包含新的对话记录）
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        if (!result.Success)
        {
            return new DialogueResponseDto
            {
                Success = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        // 5. 获取最后一个 AI 响应
        var aiTurn = result.NewTurns.LastOrDefault();
        if (aiTurn == null)
        {
            return new DialogueResponseDto
            {
                Success = false,
                ErrorMessage = "No response generated"
            };
        }

        return new DialogueResponseDto
        {
            Success = true,
            SpeakerId = aiTurn.SpeakerId.ToString(),
            SpeakerName = aiTurn.SpeakerName,
            Content = aiTurn.Content,
            Type = aiTurn.Type.ToString(),
            Timestamp = aiTurn.Timestamp
        };
    }

    public async Task<IReadOnlyList<DialogueTurnDto>> GetHistoryAsync(GetHistoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {request.SessionId} not found");

        var history = session.GetRecentHistory(request.Count);
        return history.Select(h => h.ToDto()).ToList();
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        SendMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Streaming message in session {SessionId} from player {PlayerId}",
            request.SessionId, request.PlayerId);

        // 先处理消息获取完整响应
        var response = await SendMessageAsync(request, cancellationToken);

        if (!response.Success)
        {
            yield return $"Error: {response.ErrorMessage}";
            yield break;
        }

        // 模拟流式输出：逐字返回
        var content = response.Content;
        var buffer = new StringBuilder();

        foreach (var chunk in ChunkText(content, 2))
        {
            yield return chunk;
            await Task.Delay(20, cancellationToken); // 模拟打字延迟
        }
    }

    private static IEnumerable<string> ChunkText(string text, int chunkSize)
    {
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }
    }
}
