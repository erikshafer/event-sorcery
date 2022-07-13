using EventSorcery.Meta;
using EventSorcery.Types;

namespace EventSorcery.Aggregates;

public interface IDomainEvent
{
    Type AggregateType { get; }
    Type IdentityType { get; }
    Type EventType { get; }
    ulong AggregateSequenceNumber { get; }
    Metadata Metadata { get; }
    DateTimeOffset Timestamp { get; }
}

public interface IDomainEvent<TAggregate, out TIdentity> : IDomainEvent
    where TAggregate : Aggregate
    where TIdentity : AggregateId
{
    TIdentity AggregateIdentity { get; }
}

public interface IDomainEvent<TAggregate, out TIdentity, out TAggregateEvent> : IDomainEvent<TAggregate, TIdentity>
    where TAggregate : Aggregate
    where TIdentity : AggregateId
    where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
{
    TAggregateEvent AggregateEvent { get; }
}

public class DomainEvent<TAggregate, TIdentity, TAggregateEvent> : IDomainEvent<TAggregate, TIdentity, TAggregateEvent>
    where TAggregate : Aggregate
    where TIdentity : AggregateId
    where TAggregateEvent : class, IAggregateEvent<TAggregate, TIdentity>
{
    public Type AggregateType => typeof(TAggregate);
    public Type IdentityType => typeof(TIdentity);
    public Type EventType => typeof(TAggregateEvent);
    
    public TIdentity AggregateIdentity { get; }
    public TAggregateEvent AggregateEvent { get; }
    public ulong AggregateSequenceNumber { get; }
    public Metadata Metadata { get; }
    public DateTimeOffset Timestamp { get; }
    
    public DomainEvent(
        TIdentity aggregateIdentity,
        TAggregateEvent aggregateEvent,
        Metadata metadata,
        DateTimeOffset timestamp,
        ulong aggregateSequenceNumber)
    {
        if (aggregateEvent == null) throw new ArgumentNullException(nameof(aggregateEvent));
        if (metadata == null) throw new ArgumentNullException(nameof(metadata));
        if (timestamp == default(DateTimeOffset)) throw new ArgumentNullException(nameof(timestamp));
        if (aggregateIdentity == null || string.IsNullOrEmpty(aggregateIdentity)) throw new ArgumentNullException(nameof(aggregateIdentity));
        if (aggregateSequenceNumber <= 0) throw new ArgumentOutOfRangeException(nameof(aggregateSequenceNumber));

        AggregateEvent = aggregateEvent;
        Metadata = metadata;
        Timestamp = timestamp;
        AggregateIdentity = aggregateIdentity;
        AggregateSequenceNumber = aggregateSequenceNumber;
    }

    public AggregateId GetIdentity()
    {
        return AggregateIdentity;
    }

    public IAggregateEvent GetAggregateEvent()
    {
        return AggregateEvent;
    }

    public override string ToString()
    {
        return $"{AggregateType.PrettyPrint()} v{AggregateSequenceNumber}/{EventType.PrettyPrint()}:{AggregateIdentity}";
    }
}
