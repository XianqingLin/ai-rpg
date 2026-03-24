using AI_RPG.Application.DTOs;
using AI_RPG.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI_RPG.WebAPI.Controllers;

/// <summary>
/// 会话管理 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionAppService _sessionService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(
        ISessionAppService sessionService,
        ILogger<SessionsController> logger)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SessionDto>> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionService.CreateSessionAsync(request, cancellationToken);
            _logger.LogInformation("Session created: {SessionId}", session.Id);
            return CreatedAtAction(nameof(GetSession), new { sessionId = session.Id }, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session");
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 获取会话详情
    /// </summary>
    [HttpGet("{sessionId}")]
    public async Task<ActionResult<SessionDto>> GetSession(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetSessionAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound(new { Error = $"Session {sessionId} not found" });
        }
        return Ok(session);
    }

    /// <summary>
    /// 获取用户的会话列表
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IReadOnlyList<SessionSummaryDto>>> GetUserSessions(
        string userId,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionService.GetUserSessionsAsync(userId, cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// 玩家加入会话
    /// </summary>
    [HttpPost("{sessionId}/join")]
    public async Task<ActionResult<SessionDto>> JoinSession(
        string sessionId,
        [FromBody] JoinSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 确保请求中的 sessionId 与 URL 一致
            var joinRequest = new JoinSessionRequest
            {
                SessionId = sessionId,
                UserId = request.UserId,
                PlayerName = request.PlayerName
            };

            var session = await _sessionService.JoinSessionAsync(joinRequest, cancellationToken);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 开始会话
    /// </summary>
    [HttpPost("{sessionId}/start")]
    public async Task<ActionResult<SessionDto>> StartSession(
        string sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionService.StartSessionAsync(sessionId, cancellationToken);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 暂停会话
    /// </summary>
    [HttpPost("{sessionId}/pause")]
    public async Task<ActionResult<SessionDto>> PauseSession(
        string sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionService.PauseSessionAsync(sessionId, cancellationToken);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 结束会话
    /// </summary>
    [HttpPost("{sessionId}/end")]
    public async Task<IActionResult> EndSession(
        string sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sessionService.EndSessionAsync(sessionId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 添加 NPC 到会话
    /// </summary>
    [HttpPost("{sessionId}/npcs")]
    public async Task<ActionResult<NPCDto>> AddNPC(
        string sessionId,
        [FromBody] AddNPCRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var npc = await _sessionService.AddNPCAsync(sessionId, request, cancellationToken);
            _logger.LogInformation("NPC {NPCName} added to session {SessionId}", npc.Name, sessionId);
            return CreatedAtAction(nameof(GetSession), new { sessionId }, npc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add NPC to session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 移除 NPC
    /// </summary>
    [HttpDelete("{sessionId}/npcs/{npcId}")]
    public async Task<IActionResult> RemoveNPC(
        string sessionId,
        string npcId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sessionService.RemoveNPCAsync(sessionId, npcId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove NPC {NPCId} from session {SessionId}", npcId, sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 切换场景
    /// </summary>
    [HttpPost("{sessionId}/scene")]
    public async Task<ActionResult<SessionDto>> SwitchScene(
        string sessionId,
        [FromBody] SwitchSceneRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionService.SwitchSceneAsync(sessionId, request, cancellationToken);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch scene in session {SessionId}", sessionId);
            return BadRequest(new { Error = ex.Message });
        }
    }
}
