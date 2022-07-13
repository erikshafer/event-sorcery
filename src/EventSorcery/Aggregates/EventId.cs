using EventSorcery.Exceptions;

namespace EventSorcery.Aggregates;

public abstract record EventId 
{
    protected EventId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEventIdException(this);

        Value = value;
    }
    
    private string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(EventId id) => id.Value;

    public void Deconstruct(out string value) => value = Value;
}