namespace EventSorcery.Aggregates;

public abstract class Aggregate
{
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
    
}