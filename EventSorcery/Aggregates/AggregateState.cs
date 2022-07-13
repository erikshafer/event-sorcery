namespace EventSorcery.Aggregates;

public abstract record AggregateState<T> where T : AggregateState<T>
{
    private readonly Dictionary<Type, Func<T, object, T>> _handlers = new();
    
    public virtual T When(object @event)
    {
        var eventType = @event.GetType();

        if (!_handlers.TryGetValue(eventType, out var handler))
            return (T)this;
        
        return handler((T)this, @event);
    }

    public void On<TEvent>(Func<T, TEvent, T> handle)
    {
        if (!_handlers.TryAdd(typeof(TEvent), (state, @event) => handle(state, (TEvent)@event)))
            throw new InvalidOperationException($"Duplicate handler found for {typeof(TEvent).Name}");
    }
}

public abstract record AggregateState<T, TId> : AggregateState<T>
    where T : AggregateState<T, TId>
    where TId : AggregateId 
{
    public TId Id { get; protected init; } = null!;

    internal T SetId(TId id) => (T)this with { Id = id };
}