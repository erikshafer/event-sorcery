using EventSorcery.Aggregates;

namespace EventSorcery.Exceptions;

public class InvalidAggregateIdException : Exception
{
    public InvalidAggregateIdException(AggregateId id)
        : this(id.GetType())
    {
    }

    private InvalidAggregateIdException(Type idType)
        : base($"Aggregate identity {idType.Name} cannot have an empty value")
    {
    }
}
