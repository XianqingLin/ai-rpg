namespace AI_RPG.Domain.Entities;

/// <summary>
/// NPC角色设定
/// </summary>
public sealed class NPCProfile
{
    /// <summary>
    /// 外貌描述
    /// </summary>
    public string Appearance { get; }

    /// <summary>
    /// 性格特点
    /// </summary>
    public string Personality { get; }

    /// <summary>
    /// 背景故事
    /// </summary>
    public string Background { get; }

    public NPCProfile(string appearance, string personality, string background)
    {
        Appearance = appearance ?? throw new ArgumentNullException(nameof(appearance));
        Personality = personality ?? throw new ArgumentNullException(nameof(personality));
        Background = background ?? throw new ArgumentNullException(nameof(background));
    }
}
