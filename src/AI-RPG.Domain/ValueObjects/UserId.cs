namespace AI_RPG.Domain.ValueObjects;

/// <summary>
/// 用户ID值对象
/// </summary>
public sealed class UserId : ValueObject
{
    public string Value { get; }

    public UserId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("User ID cannot be empty", nameof(value));

        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid().ToString("N"));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
