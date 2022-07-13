namespace EventSorcery.Stores;

public interface IEventWriter
{
    Task<AppendEventsResult> AppendEvents(
        StreamName stream,
        ExpectedStreamVersion expectedVersion,
        IReadOnlyCollection<StreamEvent> events,
        CancellationToken cancellationToken);
}