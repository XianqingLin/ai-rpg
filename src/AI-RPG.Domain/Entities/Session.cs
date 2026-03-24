using AI_RPG.Domain.Events;
using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Domain.Entities;

/// <summary>
/// 会话聚合根
/// </summary>
public sealed class Session
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public SessionId Id { get; }

    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 当前状态
    /// </summary>
    public SessionStatus Status { get; private set; }

    /// <summary>
    /// 游戏设定
    /// </summary>
    public GameSetting Setting { get; }

    /// <summary>
    /// 当前场景
    /// </summary>
    public Scene CurrentScene { get; private set; }

    /// <summary>
    /// 参与者集合
    /// </summary>
    private readonly List<Participant> _participants = new();
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    /// <summary>
    /// 对话历史
    /// </summary>
    private readonly List<DialogueTurn> _dialogueHistory = new();
    public IReadOnlyList<DialogueTurn> DialogueHistory => _dialogueHistory.AsReadOnly();

    /// <summary>
    /// 当前回合数
    /// </summary>
    public int CurrentTurnNumber => _dialogueHistory.Count;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 领域事件集合
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Session(
        SessionId id,
        string title,
        GameSetting setting,
        Scene initialScene,
        GameMaster gameMaster)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Setting = setting ?? throw new ArgumentNullException(nameof(setting));
        CurrentScene = initialScene ?? throw new ArgumentNullException(nameof(initialScene));
        Status = SessionStatus.Preparing;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;

        // 添加主持人
        _participants.Add(gameMaster ?? throw new ArgumentNullException(nameof(gameMaster)));
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    public static Session Create(
        string title,
        GameSetting setting,
        Scene initialScene,
        GameMaster? gameMaster = null)
    {
        return new Session(
            SessionId.New(),
            title,
            setting,
            initialScene,
            gameMaster ?? GameMaster.CreateDefault());
    }

    /// <summary>
    /// 开始会话
    /// </summary>
    public void Start()
    {
        if (Status != SessionStatus.Preparing)
            throw new InvalidOperationException("Session can only be started from Preparing status");

        Status = SessionStatus.Running;
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new SessionStarted(Id));
    }

    /// <summary>
    /// 暂停会话
    /// </summary>
    public void Pause()
    {
        if (Status != SessionStatus.Running)
            throw new InvalidOperationException("Can only pause a running session");

        Status = SessionStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 恢复会话
    /// </summary>
    public void Resume()
    {
        if (Status != SessionStatus.Paused)
            throw new InvalidOperationException("Can only resume a paused session");

        Status = SessionStatus.Running;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 结束会话
    /// </summary>
    public void End(string reason)
    {
        if (Status == SessionStatus.Ended)
            throw new InvalidOperationException("Session is already ended");

        Status = SessionStatus.Ended;
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new SessionEnded(Id, reason ?? "会话结束"));
    }

    /// <summary>
    /// 添加玩家
    /// </summary>
    public void AddPlayer(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));
        if (_participants.Any(p => p.Id == player.Id))
            throw new InvalidOperationException("Player is already in the session");

        _participants.Add(player);
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new PlayerJoined(Id, player.Id, player.Name));
    }

    /// <summary>
    /// 添加NPC
    /// </summary>
    public void AddNPC(NPC npc)
    {
        if (npc == null) throw new ArgumentNullException(nameof(npc));
        if (_participants.Any(p => p.Id == npc.Id))
            throw new InvalidOperationException("NPC is already in the session");

        _participants.Add(npc);
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new NPCEnteredScene(Id, npc.Id, npc.Name));
    }

    /// <summary>
    /// 移除参与者
    /// </summary>
    public void RemoveParticipant(ParticipantId participantId, string reason)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null)
            throw new InvalidOperationException("Participant not found");

        _participants.Remove(participant);
        UpdatedAt = DateTime.UtcNow;

        if (participant is Player player)
        {
            _domainEvents.Add(new PlayerLeft(Id, player.Id, reason));
        }
        else if (participant is NPC npc)
        {
            _domainEvents.Add(new NPCLeftScene(Id, npc.Id));
        }
    }

    /// <summary>
    /// 切换场景
    /// </summary>
    public void SwitchScene(Scene newScene)
    {
        CurrentScene = newScene ?? throw new ArgumentNullException(nameof(newScene));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 记录对话
    /// </summary>
    public DialogueTurn RecordDialogue(ParticipantId speakerId, string content, DialogueType type)
    {
        var speaker = _participants.FirstOrDefault(p => p.Id == speakerId)
            ?? throw new InvalidOperationException("Speaker not found in session");

        var turn = new DialogueTurn(
            CurrentTurnNumber + 1,
            speakerId,
            speaker.Name,
            content,
            type);

        _dialogueHistory.Add(turn);
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new DialogueSpoken(
            Id,
            turn.TurnNumber,
            speakerId,
            speaker.Name,
            content,
            type));

        return turn;
    }

    /// <summary>
    /// 获取最近的对话历史
    /// </summary>
    public IReadOnlyList<DialogueTurn> GetRecentHistory(int count)
    {
        return _dialogueHistory
            .TakeLast(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// 获取玩家列表
    /// </summary>
    public IReadOnlyList<Player> GetPlayers()
    {
        return _participants
            .OfType<Player>()
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// 获取NPC列表
    /// </summary>
    public IReadOnlyList<NPC> GetNPCs()
    {
        return _participants
            .OfType<NPC>()
            .Where(n => n.IsPresent)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// 获取主持人
    /// </summary>
    public GameMaster GetGameMaster()
    {
        return _participants
            .OfType<GameMaster>()
            .FirstOrDefault()
            ?? throw new InvalidOperationException("GameMaster not found");
    }

    /// <summary>
    /// 构建AI上下文
    /// </summary>
    public string BuildContextForAI()
    {
        var context = new System.Text.StringBuilder();

        // 世界观设定
        context.AppendLine($"游戏类型：{Setting.Genre}");
        context.AppendLine($"主题：{Setting.Theme}");
        context.AppendLine($"世界观：{Setting.WorldDescription}");
        context.AppendLine();

        // 当前场景
        context.AppendLine($"当前场景：{CurrentScene.Name}");
        context.AppendLine($"场景描述：{CurrentScene.Description}");
        context.AppendLine();

        // 在场角色
        context.AppendLine("在场角色：");
        foreach (var participant in _participants.Where(p => p.State == ParticipantState.Active))
        {
            context.AppendLine($"- {participant.Name}");
        }
        context.AppendLine();

        // 最近对话
        context.AppendLine("最近对话：");
        foreach (var turn in GetRecentHistory(10))
        {
            context.AppendLine($"{turn.SpeakerName}: {turn.Content}");
        }

        return context.ToString();
    }

    /// <summary>
    /// 清除领域事件
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
