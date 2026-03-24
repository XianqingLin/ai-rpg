namespace AI_RPG.Domain.ValueObjects;

/// <summary>
/// 参与者ID值对象
/// </summary>
public sealed class ParticipantId : ValueObject
{
    public string Value { get; }

    public ParticipantId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Participant ID cannot be empty", nameof(value));

        Value = value;
    }

    public static ParticipantId New() => new(Guid.NewGuid().ToString("N"));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
