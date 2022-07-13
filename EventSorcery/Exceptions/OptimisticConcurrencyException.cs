using EventSorcery.Aggregates;
using EventSorcery.Stores;

namespace EventSorcery.Exceptions;

public class OptimisticConcurrencyException : Exception
{
    public OptimisticConcurrencyException(Type aggregateType, StreamName streamName, Exception inner)
        : base($"Update of {aggregateType.Name} failed due to the wrong version in stream {streamName}."
               + $" {inner.Message} {inner.InnerException?.Message}")
    {
    }
}

public class OptimisticConcurrencyException<T> : OptimisticConcurrencyException
    where T : Aggregate
{
    public OptimisticConcurrencyException(StreamName streamName, Exception inner)
        : base(typeof(T), streamName, inner)
    {
    }
}