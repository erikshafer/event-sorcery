namespace EventSorcery.Aggregates;

public abstract class Aggregate
{
    public abstract string GetId();
    
    public int OriginalVersion { get; protected set; } = -1;
    public int CurrentVersion { get; protected set; } = -1;
    
    
    private readonly List<object> _changes = new();
    
    public IReadOnlyCollection<object> Changes => _changes.AsReadOnly();

    public void ClearChanges() => _changes.Clear();

    public abstract void Load(IEnumerable<object> events);

    public abstract void Fold(object @event);

    public void AddChange(object @event) => _changes.Add(@event);
}

public abstract class Aggregate<T> : Aggregate
    where T : AggregateState<T>, new()
{
    public T State { get; internal set; }
    
    private T Fold(T state, object evt) {
        OriginalVersion++;
        CurrentVersion++;
        return state.When(evt);
    }
    
    protected virtual (T PreviousState, T CurrentState) Apply(object evt)
    {
        AddChange(evt);
        var previous = State;
        State = State.When(evt);
        CurrentVersion++;
        return (previous, State);
    }
    
    public override void Load(IEnumerable<object?> events)
        => State = events.Where(x => x != null).Aggregate(new T(), Fold!);

    public override void Fold(object evt) => State = Fold(State, evt);
}

public abstract class Aggregate<T, TId> : Aggregate<T>
    where T : AggregateState<T, TId>, new()
    where TId : AggregateId
{
    public override string GetId() => State.Id;
}