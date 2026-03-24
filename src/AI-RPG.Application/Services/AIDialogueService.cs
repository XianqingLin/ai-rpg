using AI_RPG.AICapabilities.LLM;
using AI_RPG.Domain.Entities;
using AI_RPG.Domain.Repositories;
using AI_RPG.Domain.Services;
using AI_RPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AI_RPG.Application.Services;

/// <summary>
/// 基于 AI 的对话服务实现
/// </summary>
public sealed class AIDialogueService : IDialogueService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILLMClient _llmClient;
    private readonly ILogger<AIDialogueService> _logger;

    public AIDialogueService(
        ISessionRepository sessionRepository,
        ILLMClient llmClient,
        ILogger<AIDialogueService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DialogueResult> ProcessPlayerInputAsync(
        SessionId sessionId,
        ParticipantId playerId,
        string input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(playerId);
        ArgumentException.ThrowIfNullOrEmpty(input);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        var player = session.GetPlayers().FirstOrDefault(p => p.Id == playerId)
            ?? throw new InvalidOperationException($"Player {playerId} not found in session");

        _logger.LogDebug("Processing player input: {Input}", input);

        // 1. 记录玩家输入
        var playerTurn = session.RecordDialogue(playerId, input, DialogueType.Speech);

        // 2. 决定响应者（简单策略：GM 响应）
        // 可以扩展为：@NPC名字 时由特定NPC响应
        var responder = DecideResponder(session, input);

        // 3. 构建 Prompt
        var prompt = BuildPrompt(session, responder, input);

        // 4. 调用 AI 生成响应
        string aiResponse;
        try
        {
            aiResponse = await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI response");
            return new DialogueResult
            {
                Success = false,
                ErrorMessage = "AI service error: " + ex.Message
            };
        }

        // 5. 记录 AI 响应
        var aiTurn = session.RecordDialogue(responder.Id, aiResponse.Trim(), DialogueType.Speech);

        _logger.LogDebug("AI response generated: {Response}", aiResponse);

        return new DialogueResult
        {
            Success = true,
            Response = aiResponse,
            Type = DialogueType.Speech,
            NewTurns = new List<DialogueTurn> { playerTurn, aiTurn }
        };
    }

    public async Task<DialogueResult> GenerateNPCResponseAsync(
        SessionId sessionId,
        ParticipantId npcId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        var npc = session.GetNPCs().FirstOrDefault(n => n.Id == npcId)
            ?? throw new InvalidOperationException($"NPC {npcId} not found in session");

        // 构建 NPC 特定的 Prompt
        var prompt = BuildNPCPrompt(session, npc);

        string aiResponse;
        try
        {
            aiResponse = await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate NPC response");
            return new DialogueResult
            {
                Success = false,
                ErrorMessage = "AI service error: " + ex.Message
            };
        }

        var aiTurn = session.RecordDialogue(npcId, aiResponse.Trim(), DialogueType.Speech);

        return new DialogueResult
        {
            Success = true,
            Response = aiResponse,
            Type = DialogueType.Speech,
            NewTurns = new List<DialogueTurn> { aiTurn }
        };
    }

    public async Task<DialogueResult> GenerateNarrationAsync(
        SessionId sessionId,
        string context,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        var gm = session.GetGameMaster();

        // 构建叙述 Prompt
        var prompt = $@"
你是一位专业的游戏主持人（GM），正在主持一场{session.Setting.Genre}类型的跑团游戏。

世界观设定：
{session.Setting.WorldDescription}

当前场景：{session.CurrentScene.Name}
{session.CurrentScene.Description}

上下文：
{context}

请以游戏主持人的身份，用第二人称描述当前情况，推动剧情发展。保持叙述简洁生动，不超过200字。
";

        string aiResponse;
        try
        {
            aiResponse = await _llmClient.SendMessageAsync(prompt, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate narration");
            return new DialogueResult
            {
                Success = false,
                ErrorMessage = "AI service error: " + ex.Message
            };
        }

        var narrationTurn = session.RecordDialogue(gm.Id, aiResponse.Trim(), DialogueType.Narration);

        return new DialogueResult
        {
            Success = true,
            Response = aiResponse,
            Type = DialogueType.Narration,
            NewTurns = new List<DialogueTurn> { narrationTurn }
        };
    }

    private Participant DecideResponder(Session session, string input)
    {
        // 简单策略：检查是否 @ 了某个 NPC
        // 例如："@村长 你好" → 村长响应
        var npcs = session.GetNPCs();
        foreach (var npc in npcs)
        {
            if (input.Contains($"@{npc.Name}") || input.Contains($"@{npc.Name}"))
            {
                return npc;
            }
        }

        // 默认由 GM 响应
        return session.GetGameMaster();
    }

    private string BuildPrompt(Session session, Participant responder, string playerInput)
    {
        var context = session.BuildContextForAI();

        string roleDescription;
        if (responder is NPC npc)
        {
            roleDescription = $@"
你是 NPC：{npc.Name}
外貌：{npc.Profile.Appearance}
性格：{npc.Profile.Personality}
背景：{npc.Profile.Background}

请以这个角色的身份回应玩家，保持角色设定的一致性。
";
        }
        else if (responder is GameMaster)
        {
            roleDescription = @"
你是游戏主持人（GM）。
你的职责是：
1. 描述场景和环境
2. 扮演 NPC（当玩家与他们对话时）
3. 推进剧情发展
4. 裁定玩家的行动

请以主持人的身份回应玩家。
";
        }
        else
        {
            roleDescription = "";
        }

        return $@"
{context}

{roleDescription}

玩家说：{playerInput}

请回应：
";
    }

    private string BuildNPCPrompt(Session session, NPC npc)
    {
        var context = session.BuildContextForAI();

        return $@"
{context}

你是 NPC：{npc.Name}
外貌：{npc.Profile.Appearance}
性格：{npc.Profile.Personality}
背景：{npc.Profile.Background}

当前场景中有玩家和其他角色。请根据当前情况，主动说些什么或做出反应。
保持角色设定，回应简洁自然。
";
    }
}
