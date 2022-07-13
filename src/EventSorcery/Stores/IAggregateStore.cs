using EventSorcery.Aggregates;

namespace EventSorcery.Stores;

public interface IAggregateStore
{
    public Task<AppendEventsResult> Store<T, TId>(T aggregate, TId id, CancellationToken cancellationToken) 
        where T : Aggregate where TId : AggregateId =>
        this.Store<T>(StreamName.For<T, TId>(id), aggregate, cancellationToken);
    
    Task<AppendEventsResult> Store<T>(StreamName streamName, T aggregate, CancellationToken cancellationToken) 
        where T : Aggregate;
    
    public Task<T> Load<T, TId>(TId id, CancellationToken cancellationToken) 
        where T : Aggregate where TId : AggregateId =>
        this.Load<T>(StreamName.For<T, TId>(id), cancellationToken);
    
    Task<T> Load<T>(StreamName streamName, CancellationToken cancellationToken) 
        where T : Aggregate;

    public Task<T> LoadOrNew<T, TId>(TId id, CancellationToken cancellationToken) 
        where T : Aggregate where TId : AggregateId =>
        this.LoadOrNew<T>(StreamName.For<T, TId>(id), cancellationToken);
    
    Task<T> LoadOrNew<T>(StreamName streamName, CancellationToken cancellationToken) 
        where T : Aggregate;
}