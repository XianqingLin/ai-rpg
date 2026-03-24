namespace AI_RPG.Application.DTOs;

/// <summary>
/// 玩家 DTO
/// </summary>
public sealed class PlayerDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}

/// <summary>
/// NPC 设定 DTO
/// </summary>
public sealed class NPCProfileDto
{
    public string Appearance { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
}

/// <summary>
/// NPC DTO
/// </summary>
public sealed class NPCDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public NPCProfileDto Profile { get; set; } = new();
    public bool IsPresent { get; set; }
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// 添加 NPC 请求
/// </summary>
public sealed class AddNPCRequest
{
    public string Name { get; set; } = string.Empty;
    public string Appearance { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
}

/// <summary>
/// 加入会话请求
/// </summary>
public sealed class JoinSessionRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
}
