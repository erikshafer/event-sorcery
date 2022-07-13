using System.Collections.Concurrent;

namespace EventSorcery.Aggregates;

public delegate T AggregateFactory<out T>() where T : Aggregate;

public class AggregateFactoryRegistry
{
    public static readonly AggregateFactoryRegistry Instance = new();

    private readonly ConcurrentDictionary<Type, Func<Aggregate>> _registry = new();
    
    public AggregateFactoryRegistry CreateAggregateUsing<T>(AggregateFactory<T> factory)
        where T : Aggregate
    {
        _registry.TryAdd(typeof(T), () => factory());
        return this;
    }

    public void UnsafeCreateAggregateUsing<T>(Type type, Func<Aggregate> factory)
        where T : Aggregate
    {
        _registry.TryAdd(type, factory);
    }
    
    public void UnsafeCreateAggregateUsing<T>(Func<Aggregate> factory)
        where T : Aggregate
    {
        var type = typeof(T);
        _registry.TryAdd(type, factory);
    }
    
    public void UnsafeCreateAggregateUsing(Type type, Func<Aggregate> factory)
    {
        _registry.TryAdd(type, factory);
    }

    internal T CreateInstance<T, TState>()
        where T : Aggregate<TState>
        where TState : AggregateState<TState>, new() {
        var instance = _registry.TryGetValue(typeof(T), out var factory)
            ? (T)factory()
            : Activator.CreateInstance<T>();

        return instance;
    }

    internal T CreateInstance<T>() where T : Aggregate {
        var instance = _registry.TryGetValue(typeof(T), out var factory)
            ? (T)factory()
            : Activator.CreateInstance<T>();

        return instance;
    }
    
    // internal T CreateInstance<T, TState, TId>()
    //     where T : Aggregate<TState, TId>
    //     where TState : AggregateState<TState, TId>, new()
    //     where TId : AggregateId
    // {
    //     return _registry.TryGetValue(typeof(T), out var factory)
    //         ? (T)factory()
    //         : Activator.CreateInstance<T>();
    // }

    // internal T CreateInstance<T>() where T : Aggregate
    // {
    //     return _registry.TryGetValue(typeof(T), out var factory)
    //         ? (T)factory()
    //         : Activator.CreateInstance<T>();
    // }
}
