using AI_RPG.Application.DTOs;
using AI_RPG.Application.Interfaces;
using AI_RPG.Application.Mappings;
using AI_RPG.Domain.Entities;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AI_RPG.Application.Services;

/// <summary>
/// 会话应用服务实现
/// </summary>
public sealed class SessionAppService : ISessionAppService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionAppService> _logger;

    public SessionAppService(
        ISessionRepository sessionRepository,
        ILogger<SessionAppService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SessionDto> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Creating new session: {Title}", request.Title);

        var setting = new GameSetting(request.Genre, request.Theme, request.WorldDescription);
        var scene = new Scene(request.InitialScene.Name, request.InitialScene.Description);
        var gameMaster = GameMaster.CreateDefault();

        var session = Session.Create(request.Title, setting, scene, gameMaster);

        await _sessionRepository.AddAsync(session, cancellationToken);

        _logger.LogInformation("Session created: {SessionId}", session.Id);

        return session.ToDto();
    }

    public async Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken);
        return session?.ToDto();
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var sessions = await _sessionRepository.GetActiveByUserAsync(userId, cancellationToken);
        return sessions.Select(s => s.ToSummaryDto()).ToList();
    }

    public async Task<SessionDto> JoinSessionAsync(JoinSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("User {UserId} joining session {SessionId}", request.UserId, request.SessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {request.SessionId} not found");

        var player = Player.Create(request.PlayerName, request.UserId);
        session.AddPlayer(player);

        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("User {UserId} joined session {SessionId} as {PlayerName}",
            request.UserId, request.SessionId, request.PlayerName);

        return session.ToDto();
    }

    public async Task<NPCDto> AddNPCAsync(string sessionId, AddNPCRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Adding NPC {NPCName} to session {SessionId}", request.Name, sessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        var profile = new NPCProfile(request.Appearance, request.Personality, request.Background);
        var npc = NPC.Create(request.Name, profile);

        session.AddNPC(npc);
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("NPC {NPCName} added to session {SessionId}", request.Name, sessionId);

        return npc.ToDto();
    }

    public async Task RemoveNPCAsync(string sessionId, string npcId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(npcId);

        _logger.LogInformation("Removing NPC {NPCId} from session {SessionId}", npcId, sessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.RemoveParticipant(new ParticipantId(npcId), "NPC removed by user");
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("NPC {NPCId} removed from session {SessionId}", npcId, sessionId);
    }

    public async Task<SessionDto> SwitchSceneAsync(string sessionId, SwitchSceneRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Switching scene in session {SessionId} to {SceneName}", sessionId, request.Name);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        var newScene = new Scene(request.Name, request.Description);
        session.SwitchScene(newScene);

        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return session.ToDto();
    }

    public async Task<SessionDto> StartSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);

        _logger.LogInformation("Starting session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.Start();
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Session {SessionId} started", sessionId);

        return session.ToDto();
    }

    public async Task<SessionDto> PauseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);

        _logger.LogInformation("Pausing session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.Pause();
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Session {SessionId} paused", sessionId);

        return session.ToDto();
    }

    public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);

        _logger.LogInformation("Ending session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(new SessionId(sessionId), cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.End("Session ended by user");
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Session {SessionId} ended", sessionId);
    }
}
