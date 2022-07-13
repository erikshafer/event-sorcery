using EventSorcery.Aggregates;
using EventSorcery.Exceptions;
using EventSorcery.Tools;

namespace EventSorcery.Stores;

/// <summary>
/// Designed for use with EventStoreDB. Need to think about making it agnostic
/// as possible at a later time,  but for now allow it to easily work with Marten.
/// </summary>
public record StreamName
{
    public string Value { get; }

    public StreamName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidStreamName(value);
            
        Value = value;
    }

    public static StreamName For<T>(string entityId) => new($"{typeof(T).Name}-{Ensure.NotEmptyString(entityId)}");

    public static StreamName For<T, TId>(TId entityId)
        where T : Aggregate where TId : AggregateId
        => new($"{typeof(T).Name}-{Ensure.NotEmptyString(entityId.ToString())}");

    public static StreamName For<T, TState, TId>(TId entityId)
        where T : Aggregate<TState> where TState : AggregateState<TState>, new() where TId : AggregateId
        => new($"{typeof(T).Name}-{Ensure.NotEmptyString(entityId.ToString())}");

    public string GetId() => Value[(Value.IndexOf("-", StringComparison.InvariantCulture) + 1)..];
    
    public static implicit operator string(StreamName streamName) => streamName.Value;

    public override string ToString() => Value;
}