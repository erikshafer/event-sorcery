using EventSorcery.Aggregates;
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

    public static StreamName For<T, TState, TId>(TId aggregateId)
        where T : Aggregate<TState, TId>
        where TState : AggregateState<TState, TId>, new()
        where TId : AggregateId => For<T>(aggregateId);
        
    public static implicit operator string(StreamName streamName) => streamName.Value;

    public override string ToString() => Value;
}

public class InvalidStreamName : Exception {
    public InvalidStreamName(string? streamName)
        : base($"Stream name is {(string.IsNullOrWhiteSpace(streamName) ? "empty" : "invalid")}") { }
}