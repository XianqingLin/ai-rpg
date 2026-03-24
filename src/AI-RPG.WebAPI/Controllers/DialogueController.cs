using System.Runtime.CompilerServices;
using AI_RPG.Application.DTOs;
using AI_RPG.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI_RPG.WebAPI.Controllers;

/// <summary>
/// 对话交互 API
/// </summary>
[ApiController]
[Route("api/sessions/{sessionId}/[controller]")]
public class DialogueController : ControllerBase
{
    private readonly IDialogueAppService _dialogueService;
    private readonly ILogger<DialogueController> _logger;

    public DialogueController(
        IDialogueAppService dialogueService,
        ILogger<DialogueController> logger)
    {
        _dialogueService = dialogueService ?? throw new ArgumentNullException(nameof(dialogueService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 发送消息（玩家与AI交互）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DialogueResponseDto>> SendMessage(
        string sessionId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 确保请求中的 sessionId 与 URL 一致
            var messageRequest = new SendMessageRequest
            {
                SessionId = sessionId,
                PlayerId = request.PlayerId,
                Message = request.Message
            };

            _logger.LogInformation("Processing message in session {SessionId} from player {PlayerId}",
                sessionId, request.PlayerId);

            var response = await _dialogueService.SendMessageAsync(messageRequest, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(new { Error = response.ErrorMessage });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message in session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 获取对话历史
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<DialogueTurnDto>>> GetHistory(
        string sessionId,
        [FromQuery] int count = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetHistoryRequest
            {
                SessionId = sessionId,
                Count = count
            };

            var history = await _dialogueService.GetHistoryAsync(request, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history for session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 流式发送消息（SSE）
    /// </summary>
    [HttpPost("stream")]
    public async IAsyncEnumerable<string> StreamMessage(
        string sessionId,
        [FromBody] SendMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 确保请求中的 sessionId 与 URL 一致
        var messageRequest = new SendMessageRequest
        {
            SessionId = sessionId,
            PlayerId = request.PlayerId,
            Message = request.Message
        };

        await foreach (var chunk in _dialogueService.StreamMessageAsync(messageRequest, cancellationToken))
        {
            yield return chunk;
        }
    }
}
