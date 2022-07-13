namespace EventSorcery.Stores;

public interface IEventStore : IEventReader, IEventWriter
{
    Task<bool> StreamExists(
        StreamName stream,
        CancellationToken cancellationToken);

    Task TruncateStream(
        StreamName stream,
        StreamTruncatePosition truncatePosition,
        ExpectedStreamVersion expectedVersion,
        CancellationToken cancellationToken);

    Task DeleteStream(
        StreamName stream,
        ExpectedStreamVersion expectedVersion,
        CancellationToken cancellationToken);
}