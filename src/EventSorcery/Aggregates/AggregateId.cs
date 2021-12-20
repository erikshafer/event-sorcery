namespace EventSorcery.Aggregates;

public abstract record AggregateId 
{
    protected AggregateId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exception(); // TODO

        Value = value;
    }

    private string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(AggregateId id) => id.Value;

    public void Deconstruct(out string value) => value = Value;
}