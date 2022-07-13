using EventSorcery.Aggregates;
using EventSorcery.Stores;

namespace EventSorcery.Exceptions;

public class AggregateNotFoundException : Exception
{
    public AggregateNotFoundException(Type aggregateType, StreamName streamName, Exception inner)
        : base($"Aggregate {aggregateType.Name} with not found in stream {streamName}. "
               + $"{inner.Message} {inner.InnerException?.Message}")
    {
    }
}

public class AggregateNotFoundException<T> : AggregateNotFoundException 
    where T : Aggregate
{
    public AggregateNotFoundException(StreamName streamName, Exception inner)
        : base(typeof(T), streamName, inner)
    {
    }
}