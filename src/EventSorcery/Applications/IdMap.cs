namespace EventSorcery.Applications;

public class IdMap<T> : Dictionary<Type, Func<object, CancellationToken, ValueTask<T>>>
{
    
}