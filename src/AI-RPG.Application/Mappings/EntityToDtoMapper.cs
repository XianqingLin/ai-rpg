using AI_RPG.Application.DTOs;
using AI_RPG.Domain.Entities;
using AI_RPG.Domain.ValueObjects;

namespace AI_RPG.Application.Mappings;

/// <summary>
/// 领域实体到 DTO 的映射器
/// </summary>
public static class EntityToDtoMapper
{
    public static GameSettingDto ToDto(this GameSetting setting)
    {
        return new GameSettingDto
        {
            Genre = setting.Genre,
            Theme = setting.Theme,
            WorldDescription = setting.WorldDescription
        };
    }

    public static SceneDto ToDto(this Scene scene)
    {
        return new SceneDto
        {
            Name = scene.Name,
            Description = scene.Description
        };
    }

    public static PlayerDto ToDto(this Player player)
    {
        return new PlayerDto
        {
            Id = player.Id.ToString(),
            Name = player.Name,
            UserId = player.UserId,
            State = player.State.ToString()
        };
    }

    public static NPCProfileDto ToDto(this NPCProfile profile)
    {
        return new NPCProfileDto
        {
            Appearance = profile.Appearance,
            Personality = profile.Personality,
            Background = profile.Background
        };
    }

    public static NPCDto ToDto(this NPC npc)
    {
        return new NPCDto
        {
            Id = npc.Id.ToString(),
            Name = npc.Name,
            Profile = npc.Profile.ToDto(),
            IsPresent = npc.IsPresent,
            State = npc.State.ToString()
        };
    }

    public static DialogueTurnDto ToDto(this DialogueTurn turn)
    {
        return new DialogueTurnDto
        {
            TurnNumber = turn.TurnNumber,
            SpeakerId = turn.SpeakerId.ToString(),
            SpeakerName = turn.SpeakerName,
            Content = turn.Content,
            Type = turn.Type.ToString(),
            Timestamp = turn.Timestamp
        };
    }

    public static SessionSummaryDto ToSummaryDto(this Session session)
    {
        return new SessionSummaryDto
        {
            Id = session.Id.ToString(),
            Title = session.Title,
            Status = session.Status.ToString(),
            PlayerCount = session.GetPlayers().Count,
            NPCCount = session.GetNPCs().Count,
            CreatedAt = session.CreatedAt
        };
    }

    public static SessionDto ToDto(this Session session)
    {
        return new SessionDto
        {
            Id = session.Id.ToString(),
            Title = session.Title,
            Status = session.Status.ToString(),
            Setting = session.Setting.ToDto(),
            CurrentScene = session.CurrentScene.ToDto(),
            Players = session.GetPlayers().Select(p => p.ToDto()).ToList(),
            NPCs = session.GetNPCs().Select(n => n.ToDto()).ToList(),
            RecentHistory = session.GetRecentHistory(20).Select(h => h.ToDto()).ToList(),
            CreatedAt = session.CreatedAt
        };
    }
}
