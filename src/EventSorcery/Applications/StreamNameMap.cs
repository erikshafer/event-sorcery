using EventSorcery.Aggregates;
using EventSorcery.Stores;

namespace EventSorcery.Applications;

public class StreamNameMap
{
    private readonly Dictionary<Type, Func<AggregateId, StreamName>> _map = new();

    public void Register<TId>(Func<TId, StreamName> map)
        where TId : AggregateId
        => _map.TryAdd(typeof(TId), id => map((TId)id));

    public StreamName GetStreamName<T, TId>(TId aggregateId)
        where TId : AggregateId where T : Aggregate {
        if (_map.TryGetValue(typeof(TId), out var map)) return map(aggregateId);

        _map[typeof(TId)] = id => StreamName.For<T, TId>((TId)id);

        return _map[typeof(TId)](aggregateId);
    }
}
