using EventSorcery.Aggregates;
using EventSorcery.Exceptions;
using EventSorcery.Tools;

namespace EventSorcery.Stores;

public class AggregateStore : IAggregateStore
{
    private readonly Func<StreamEvent, StreamEvent> _amendEvent;
    private readonly AggregateFactoryRegistry _factoryRegistry;
    private readonly IEventReader _eventReader;
    private readonly IEventWriter _eventWriter;

    public AggregateStore(
        IEventReader reader,
        IEventWriter writer,
        Func<StreamEvent, StreamEvent>? amendEvent = null,
        AggregateFactoryRegistry? factoryRegistry = null)
    {
        _amendEvent = amendEvent ?? (x => x);
        _factoryRegistry = factoryRegistry ?? AggregateFactoryRegistry.Instance;
        _eventReader = Ensure.NotNull(reader);
        _eventWriter = Ensure.NotNull(writer);
    }

    public AggregateStore(
        IEventStore eventStore,
        Func<StreamEvent, StreamEvent>? amendEvent = null,
        AggregateFactoryRegistry? factoryRegistry = null)
        : this(eventStore, eventStore, amendEvent, factoryRegistry)
    {
    }

    public Task<AppendEventsResult> Store<T>(StreamName streamName, T aggregate, CancellationToken cancellationToken)
        where T : Aggregate 
        => _eventWriter.Store(streamName, aggregate, _amendEvent, cancellationToken);

    public Task<T> Load<T>(StreamName streamName, CancellationToken cancellationToken)
        where T : Aggregate 
        => LoadInternal<T>(streamName, true, cancellationToken);

    public Task<T> LoadOrNew<T>(StreamName streamName, CancellationToken cancellationToken)
        where T : Aggregate
        => LoadInternal<T>(streamName, false, cancellationToken);

    private async Task<T> LoadInternal<T>(StreamName streamName, bool failIfNotFound, CancellationToken cancellationToken)
        where T : Aggregate
    {
        var aggregate = _factoryRegistry.CreateInstance<T>();

        try
        {
            var events = await _eventReader.ReadStream(
                streamName,
                StreamReadPosition.Start,
                failIfNotFound,
                cancellationToken
            );

            aggregate.Load(events.Select(x => x.Payload));
        }
        catch (StreamNotFound) when (!failIfNotFound)
        {
            return aggregate;
        }
        catch (Exception e)
        {
            // TODO: log

            throw e is StreamNotFound
                ? new AggregateNotFoundException<T>(streamName, e)
                : e;
        }

        return aggregate;
    }
}