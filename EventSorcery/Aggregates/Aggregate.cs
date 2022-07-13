using EventSorcery.Exceptions;

namespace EventSorcery.Aggregates;

public abstract class Aggregate
{
    public abstract string GetId();

    public object[] Original { get; protected set; } = Array.Empty<object>();
    
    public IReadOnlyCollection<object> Changes => _changes.AsReadOnly();
    
    public IEnumerable<object> Current => Original.Concat(_changes);
    
    public void ClearChanges() => _changes.Clear();
    
    public int OriginalVersion => Original.Length - 1;
    public int CurrentVersion => OriginalVersion + Changes.Count;

    private readonly List<object> _changes = new();
    
    public abstract void Load(IEnumerable<object> events);

    // TODO: consider reverting back to to methods called Append() and AppendChange()
    public void AddChange(object @event) => _changes.Add(@event);
    
    protected void EnsureDoesntExist(Func<Exception>? getException = null) {
        if (Original.Length > 0)
            throw getException?.Invoke()
                  ?? new DomainException($"{GetType().Name} already exists");
    }

    protected void EnsureExists(Func<Exception>? getException = null)
    {
        if (Original.Length == 0)
            throw getException?.Invoke()
                  ?? new DomainException($"{GetType().Name} doesn't exist");
    }
}

public abstract class Aggregate<T> : Aggregate
    where T : AggregateState<T>, new()
{
    protected Aggregate() => State = new T();
    
    public T State { get; private set; }
    
    // private T Fold(T state, object evt) {
    //     OriginalVersion++;
    //     CurrentVersion++;
    //     return state.When(evt);
    // }
    //
    // protected virtual (T PreviousState, T CurrentState) Apply(object evt)
    // {
    //     AddChange(evt);
    //     var previous = State;
    //     State = State.When(evt);
    //     CurrentVersion++;
    //     return (previous, State);
    // }
    //
    // public override void Load(IEnumerable<object?> events)
    //     => State = events.Where(x => x != null).Aggregate(new T(), Fold!);
    //
    // public override void Fold(object evt) => State = Fold(State, evt);
    
    /// <summary>
    /// Applies a new event to the state, adds the event to the list of pending changes,
    /// and increases the current version.
    /// </summary>
    /// <param name="evt">New domain event to be applied</param>
    /// <returns>The previous and the new aggregate states</returns>
    protected (T PreviousState, T CurrentState) Apply(object evt) {
        AddChange(evt);
        var previous = State;
        State = State.When(evt);
        return (previous, State);
    }

    /// <inheritdoc />
    public override void Load(IEnumerable<object?> events) {
        Original = events.Where(x => x != null).ToArray()!;
        State    = Original.Aggregate(new T(), Fold);
    }

    static T Fold(T state, object evt) => state.When(evt);
}

// TODO: evaluate if this is worth keeping
// public abstract class Aggregate<T, TAggregateId> : Aggregate<T>
//     where T : AggregateState<T, TAggregateId>, new()
//     where TAggregateId : AggregateId
// {
//     public override string GetId() => State.Id;
// }