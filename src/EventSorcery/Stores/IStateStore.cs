using EventSorcery.Aggregates;

namespace EventSorcery.Stores;

public interface IStateStore
{
    Task<T> LoadState<T, TId>(StreamName stream, CancellationToken cancellationToken)
        where T : AggregateState<T, TId>, new() where TId : AggregateId;
}