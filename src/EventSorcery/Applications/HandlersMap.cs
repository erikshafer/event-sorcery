namespace EventSorcery.Applications;

public record RegisteredHandler<T>(
    ExpectedState ExpectedState,
    Func<T, object, CancellationToken, ValueTask<T>> Handler);

public class HandlersMap<T> : Dictionary<Type, RegisteredHandler<T>>
{
    public void AddHandler<TCommand>(RegisteredHandler<T> handler)
    {
        if (ContainsKey(typeof(TCommand)))
        {
            // TODO: log
            throw new Exceptions.Exceptions.CommandHandlerAlreadyRegistered<TCommand>();
        }

        Add(typeof(TCommand), handler);
    }
}