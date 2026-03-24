namespace AI_RPG.Application.DTOs;

/// <summary>
/// 游戏设定 DTO
/// </summary>
public sealed class GameSettingDto
{
    public string Genre { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string WorldDescription { get; set; } = string.Empty;
}

/// <summary>
/// 场景 DTO
/// </summary>
public sealed class SceneDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 会话摘要 DTO
/// </summary>
public sealed class SessionSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int NPCCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 会话详情 DTO
/// </summary>
public sealed class SessionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public GameSettingDto Setting { get; set; } = new();
    public SceneDto CurrentScene { get; set; } = new();
    public List<PlayerDto> Players { get; set; } = new();
    public List<NPCDto> NPCs { get; set; } = new();
    public List<DialogueTurnDto> RecentHistory { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建会话请求
/// </summary>
public sealed class CreateSessionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string WorldDescription { get; set; } = string.Empty;
    public SceneDto InitialScene { get; set; } = new();
}

/// <summary>
/// 切换场景请求
/// </summary>
public sealed class SwitchSceneRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
