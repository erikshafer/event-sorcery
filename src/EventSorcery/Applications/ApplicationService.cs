using EventSorcery.Aggregates;
using EventSorcery.Stores;
using EventSorcery.Tools;
using EventSorcery.Types;

namespace EventSorcery.Applications;

public abstract class ApplicationService<T, TState, TId> 
    : IApplicationService<T, TState, TId>, IApplicationService<T> 
    where T : Aggregate<TState, TId>, new()
    where TState : AggregateState<TState, TId>, new()
    where TId : AggregateId
{
    protected IAggregateStore Store { get; }

    private readonly HandlersMap<T> _handlers = new();
    private readonly IdMap<TId>  _getId    = new();
    private readonly AggregateFactoryRegistry _factoryRegistry;
    
    public delegate void ActOnAggregate<in TCommand>(T aggregate, TCommand command);

    public delegate Task<T> ArbitraryActAsync<in TCommand>(TCommand command, CancellationToken cancellationToken);

    public delegate TId GetIdFromCommand<in TCommand>(TCommand command);

    public delegate Task<TId> GetIdFromCommandAsync<in TCommand>(TCommand command, CancellationToken cancellationToken);

    protected ApplicationService(IAggregateStore store, AggregateFactoryRegistry? factoryRegistry = null)
    {
        _factoryRegistry = factoryRegistry ?? AggregateFactoryRegistry.Instance;
        Store = store;
    }
    
    protected void OnNew<TCommand>(ActOnAggregate<TCommand> action)
        where TCommand : class
        => _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.New,
                (aggregate, cmd, _) => SyncAsTask(aggregate, cmd, action)
            )
        );
    
    protected void OnNewAsync<TCommand>(ActOnAggregateAsync<TCommand> action)
        where TCommand : class
        => _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.New,
                (aggregate, cmd, ct) => AsTask(aggregate, cmd, action, ct)
            )
        );
    
    protected void OnExisting<TCommand>(GetIdFromCommand<TCommand> getId, ActOnAggregate<TCommand> action) 
        where TCommand : class 
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.Existing,
                (aggregate, command, _) => SyncAsTask(aggregate, command, action))
        );

        _getId.TryAdd(
            typeof(TCommand),
            (command, _) => new ValueTask<TId>(getId((TCommand)command)));
    }
    
    protected void OnExistingAsync<TCommand>(GetIdFromCommand<TCommand> getId, ActOnAggregateAsync<TCommand> action)
        where TCommand : class 
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.Existing,
                (aggregate, command, token) => AsTask(aggregate, command, action, token))
        );

        _getId.TryAdd(
            typeof(TCommand),
            (command, _) => new ValueTask<TId>(getId((TCommand)command)));
    }
    
    protected void OnAny<TCommand>(GetIdFromCommand<TCommand> getId, ActOnAggregate<TCommand> action)
        where TCommand : class
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.Any,
                (aggregate, command, _) => SyncAsTask(aggregate, command, action))
        );

        _getId.TryAdd(
            typeof(TCommand),
            (command, _) => new ValueTask<TId>(getId((TCommand)command)));
    }
    
    protected void OnAnyAsync<TCommand>(GetIdFromCommand<TCommand> getId, ActOnAggregateAsync<TCommand> action)
        where TCommand : class
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.Any,
                (aggregate, command, token) => AsTask(aggregate, command, action, token))
        );

        _getId.TryAdd(
            typeof(TCommand),
            (command, _) => new ValueTask<TId>(getId((TCommand)command)));
    }
    
    protected void OnAny<TCommand>(GetIdFromCommandAsync<TCommand> getId, ActOnAggregate<TCommand> action)
        where TCommand : class
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.Any,
                (aggregate, command, _) => SyncAsTask(aggregate, command, action))
        );

        _getId.TryAdd(
            typeof(TCommand),
            async (command, token) => await getId((TCommand)command, token).NoContext()
        );
    }

    /// <summary>
    /// Register an asynchronous handler for a command, which is expected to use an a new or an existing aggregate instance.
    /// </summary>
    /// <param name="getId">Asynchronous function to get the aggregate id from the command</param>
    /// <param name="action">Asynchronous action to be performed on the aggregate,
    /// given the aggregate instance and the command</param>
    /// <typeparam name="TCommand">Command type</typeparam>
    protected void OnAnyAsync<TCommand>(GetIdFromCommandAsync<TCommand> getId, ActOnAggregateAsync<TCommand> action)
        where TCommand : class
    {
        _handlers.AddHandler<TCommand>(
            new RegisteredHandler<T>(
                ExpectedState.Any,
                (aggregate, command, token) => AsTask(aggregate, command, action, token))
        );

        _getId.TryAdd(
            typeof(TCommand),
            async (command, ct) => await getId((TCommand)command, ct).NoContext()
        );
    }
    
    protected void OnAsync<TCommand>(ArbitraryActAsync<TCommand> action)
        where TCommand : class
        => _handlers.AddHandler<TCommand>(new RegisteredHandler<T>(
            ExpectedState.Unknown, 
            async (_, command, token) => await action((TCommand)command, token).NoContext())
        );

    private static ValueTask<T> SyncAsTask<TCommand>(
        T aggregate,
        object command,
        ActOnAggregate<TCommand> action)
    {
        action(aggregate, (TCommand)command);
        return new ValueTask<T>(aggregate);
    }

    private static async ValueTask<T> AsTask<TCommand>(
        T aggregate,
        object command,
        ActOnAggregateAsync<TCommand> action,
        CancellationToken cancellationToken)
    {
        await action(aggregate, (TCommand)command, cancellationToken).NoContext();
        return aggregate;
    }
    
    public async Task<Result<TState, TId>> Handle(object command, CancellationToken cancellationToken) {
        var commandType = Ensure.NotNull(command).GetType();

        if (!_handlers.TryGetValue(commandType, out var registeredHandler)) {
            // TODO: log
            var exception = new Exceptions.Exceptions.CommandHandlerNotFound(commandType);
            return new ErrorResult<TState, TId>(exception);
        }

        try {
            var aggregate = registeredHandler.ExpectedState switch
            {
                ExpectedState.Any      => await TryLoad().NoContext(),
                ExpectedState.Existing => await Load().NoContext(),
                ExpectedState.New      => Create(),
                ExpectedState.Unknown  => default,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(registeredHandler.ExpectedState), 
                    "Unknown expected state")
            };

            var result = await registeredHandler
                .Handler(aggregate!, command, cancellationToken)
                .NoContext();

            var storeResult = await Store.Store(result, cancellationToken).NoContext();

            var changes = result.Changes.Select(x => new Change(x, TypeMap.GetTypeName(x)));
            return new OkResult<TState, TId>(result.State, changes, storeResult.GlobalPosition);
        }
        catch (Exception e)
        {
            // TODO: log

            return new ErrorResult<TState, TId>($"Error handling command {commandType.Name}", e);
        }

        async Task<T> Load() 
        {
            var id = await _getId[commandType](command, cancellationToken).NoContext();
            // return await Store.Load<T, TState, TId>(id, cancellationToken).NoContext();
            return await Store.Load<T>(id, cancellationToken).NoContext();
        }

        async Task<T> TryLoad()
        {
            var id     = await _getId[commandType](command, cancellationToken).NoContext();
            var exists = await Store.Exists<T>(id, cancellationToken).NoContext();
            return exists ? await Load().NoContext() : Create();
        }

        T Create() => _factoryRegistry.CreateInstance<T, TState, TId>();
    }

    async Task<Result> IApplicationService.Handle(object command, CancellationToken cancellationToken)
    {
        var result = await Handle(command, cancellationToken).NoContext();

        return result switch
        {
            OkResult<TState, TId>(var aggregateState, var enumerable, _) => new OkResult(aggregateState, enumerable),
            ErrorResult<TState, TId> error => new ErrorResult(error.Message, error.Exception),
            _ => throw new ApplicationException("Unknown result type")
        };
    }

    public delegate Task ActOnAggregateAsync<in TCommand>(
        T aggregate,
        TCommand command,
        CancellationToken cancellationToken);
}
