namespace EventSorcery.Aggregates;

public interface IAggregateEvent
{
    
}

public interface IAggregateEvent<TAggregate, TIdentity> : IAggregateEvent
    where TAggregate : Aggregate
    where TIdentity : AggregateId
{
}