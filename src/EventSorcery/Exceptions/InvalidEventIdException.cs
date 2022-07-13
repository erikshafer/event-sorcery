using EventSorcery.Aggregates;

namespace EventSorcery.Exceptions;

public class InvalidEventIdException : Exception
{
    public InvalidEventIdException(EventId id)
        : this(id.GetType())
    {
    }

    private InvalidEventIdException(Type idType)
        : base($"Event identity {idType.Name} cannot have an empty value")
    {
    }
}
