namespace EventSorcery.Exceptions;

public class CommandHandlerNotFound : Exception
{
    public CommandHandlerNotFound(Type type)
        : base($"Handler not found for command {type.Name}")
    {
    }
}

public class CommandHandlerNotFound<T> : CommandHandlerNotFound
{
    public CommandHandlerNotFound() : base(typeof(T))
    {
    }
}

public class CommandHandlerAlreadyRegistered<T> : Exception
{
    public CommandHandlerAlreadyRegistered()
        : base($"Command handler for ${typeof(T).Name} already registered")
    {
    }
}