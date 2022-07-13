namespace EventSorcery.Stores;

public interface IEventReader
{
    Task<StreamEvent[]> ReadEvents(
        StreamName stream,
        StreamReadPosition start,
        int count,
        CancellationToken cancellationToken);
}