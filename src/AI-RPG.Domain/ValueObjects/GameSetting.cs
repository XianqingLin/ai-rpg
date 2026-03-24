namespace AI_RPG.Domain.ValueObjects;

/// <summary>
/// 游戏设定值对象
/// </summary>
public sealed class GameSetting : ValueObject
{
    /// <summary>
    /// 游戏类型（奇幻/科幻/现代等）
    /// </summary>
    public string Genre { get; }

    /// <summary>
    /// 主题
    /// </summary>
    public string Theme { get; }

    /// <summary>
    /// 世界观描述
    /// </summary>
    public string WorldDescription { get; }

    public GameSetting(string genre, string theme, string worldDescription)
    {
        Genre = genre ?? throw new ArgumentNullException(nameof(genre));
        Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        WorldDescription = worldDescription ?? throw new ArgumentNullException(nameof(worldDescription));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Genre;
        yield return Theme;
        yield return WorldDescription;
    }
}
