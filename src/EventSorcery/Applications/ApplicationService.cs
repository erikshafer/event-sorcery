using EventSorcery.Aggregates;
using EventSorcery.Stores;
using EventSorcery.Tools;
using EventSorcery.Types;

namespace EventSorcery.Applications;

public abstract class ApplicationService<TAggregate, TState, TId>
    : IApplicationService<TAggregate, TState, TId>, IApplicationService<TAggregate>
    where TAggregate : Aggregate<TState>, new()
    where TState : AggregateState<TState>, new()
    where TId : AggregateId
{
    protected IAggregateStore Store { get; }

    private readonly HandlersMap<TAggregate> _handlers = new();
    private readonly IdMap<TId>  _idMap    = new();
    private readonly AggregateFactoryRegistry _factoryRegistry;
    private readonly StreamNameMap _streamNameMap;

    protected ApplicationService(
        IAggregateStore store,
        AggregateFactoryRegistry? factoryRegistry = null,
        StreamNameMap? streamNameMap = null)
    {
        _factoryRegistry = factoryRegistry ?? AggregateFactoryRegistry.Instance;
        _streamNameMap = streamNameMap ?? new StreamNameMap();
        Store = store;
    }
    
    public delegate Task<TAggregate> ArbitraryActAsync<in TCommand>(
        TCommand command,
        CancellationToken cancellationToken);

    protected void OnNew<TCommand>(
        GetIdFromCommand<TId, TCommand> getId,
        ActOnAggregate<TAggregate, TCommand> action)
        where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.New, action);
        _idMap.AddCommand(getId);
    }

    protected void OnNewAsync<TCommand>(
        GetIdFromCommand<TId, TCommand> getId,
        ActOnAggregateAsync<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.New, action);
        _idMap.AddCommand(getId);
    }

    protected void OnExisting<TCommand>(
        GetIdFromCommand<TId, TCommand> getId,
        ActOnAggregate<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.Existing, action);
        _idMap.AddCommand(getId);
    }

    protected void OnExistingAsync<TCommand>(
        GetIdFromCommand<TId, TCommand> getId,
        ActOnAggregateAsync<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.Existing, action);
        _idMap.AddCommand(getId);
    }

    protected void OnAny<TCommand>(
        GetIdFromCommand<TId, TCommand> getId,
        ActOnAggregate<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.Any, action);
        _idMap.AddCommand(getId);
    }

    protected void OnAny<TCommand>(
        GetIdFromCommandAsync<TId, TCommand> getId,
        ActOnAggregate<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.Any, action);
        _idMap.AddCommand(getId);
    }

    protected void OnAnyAsync<TCommand>(
        GetIdFromCommand<TId, TCommand> getId,
        ActOnAggregateAsync<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.Any, action);
        _idMap.AddCommand(getId);
    }

    protected void OnAnyAsync<TCommand>(
        GetIdFromCommandAsync<TId, TCommand> getId,
        ActOnAggregateAsync<TAggregate, TCommand> action
    ) where TCommand : class
    {
        _handlers.AddHandler(ExpectedState.Any, action);
        _idMap.AddCommand(getId);
    }

    protected void OnAsync<TCommand>(ArbitraryActAsync<TCommand> action)
        where TCommand : class
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<TAggregate>(
                ExpectedState.Unknown,
                async (_, cmd, ct) => await action((TCommand)cmd, ct).NoContext()));
    }

    public async Task<Result<TState>> Handle(object command, CancellationToken cancellationToken) {
        var commandType = Ensure.NotNull(command).GetType();

        if (!_handlers.TryGetValue(commandType, out var registeredHandler))
        {
            // TODO: log
            var exception = new Exceptions.CommandHandlerNotFound(commandType);
            return new ErrorResult<TState>(exception);
        }

        var hasGetIdFunction = _idMap.TryGetValue(commandType, out var getId);

        if (!hasGetIdFunction || getId == null)
        {
            // TODO: log
            var exception = new Exceptions.CommandHandlerNotFound(commandType);
            return new ErrorResult<TState>(exception);
        }

        var aggregateId = await getId(command, cancellationToken).NoContext();

        var streamName = _streamNameMap.GetStreamName<TAggregate, TId>(aggregateId);

        try 
        {
            var aggregate = registeredHandler.ExpectedState switch
            {
                ExpectedState.Any => await Store.LoadOrNew<TAggregate>(streamName, cancellationToken).NoContext(),
                ExpectedState.Existing => await Store.Load<TAggregate>(streamName, cancellationToken).NoContext(),
                ExpectedState.New     => Create(),
                ExpectedState.Unknown => default,
                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(registeredHandler.ExpectedState),
                    message: "Unknown expected state")
            };

            var result = await registeredHandler
                .Handler(aggregate!, command, cancellationToken)
                .NoContext();

            // Zero in the global position would mean nothing, so the receiver need to check the Changes.Length
            if (result.Changes.Count == 0)
                return new OkResult<TState>(
                    State: result.State,
                    Changes: Array.Empty<Change>(),
                    StreamPosition: 0);

            var storeResult = await Store.Store(
                    streamName: streamName != default ? streamName : GetAggregateStreamName(), // can be replaced with a then expression
                    aggregate: result,
                    cancellationToken: cancellationToken)
                .NoContext();

            var changes = result.Changes.Select(x => new Change(x, TypeMap.GetTypeName(x)));

            // TODO: log

            return new OkResult<TState>(result.State, changes, storeResult.GlobalPosition);
        }
        catch (Exception e)
        {
            // TODO: log
            return new ErrorResult<TState>($"Error handling command {commandType.Name}", e);
        }

        TAggregate Create() => _factoryRegistry.CreateInstance<TAggregate, TState>();

        StreamName GetAggregateStreamName() => _streamNameMap.GetStreamName<TAggregate, TId>(aggregateId);
    }

    async Task<Result> IApplicationService.Handle(object command, CancellationToken cancellationToken) {
        var result = await Handle(command, cancellationToken).NoContext();

        return result switch {
            OkResult<TState>(var aggregateState, var enumerable, _) => new OkResult(aggregateState, enumerable),
            ErrorResult<TState> error => new ErrorResult(error.Message, error.Exception),
            _ => throw new ApplicationException("Unknown result type")
        };
    }
}
