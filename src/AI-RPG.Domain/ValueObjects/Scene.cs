namespace AI_RPG.Domain.ValueObjects;

/// <summary>
/// 场景值对象
/// </summary>
public sealed class Scene : ValueObject
{
    /// <summary>
    /// 场景名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 场景描述
    /// </summary>
    public string Description { get; }

    public Scene(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Description;
    }

    public override string ToString() => Name;
}
