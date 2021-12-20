using EventSorcery.Aggregates;

namespace EventSorcery.Stores;

public interface IAggregateStore
{
    Task<AppendEventsResult> Store<T>(T aggregate, CancellationToken cancellationToken) where T : Aggregate;
    
    Task<T> Load<T>(string id, CancellationToken cancellationToken) where T : Aggregate;

    Task<bool> Exists<T>(string id, CancellationToken cancellationToken) where T : Aggregate;
}