namespace AI_RPG.Domain.ValueObjects;

/// <summary>
/// 会话ID值对象
/// </summary>
public sealed class SessionId : ValueObject
{
    public string Value { get; }

    public SessionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Session ID cannot be empty", nameof(value));

        Value = value;
    }

    public static SessionId New() => new(Guid.NewGuid().ToString("N"));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
