using EventSorcery.Aggregates;
using EventSorcery.Exceptions;
using EventSorcery.Tools;

namespace EventSorcery.Applications;

public delegate void ActOnAggregate<in TAggregate, in TCommand>(TAggregate aggregate, TCommand command)
    where TAggregate : Aggregate;

public delegate Task ActOnAggregateAsync<in TAggregate, in TCommand>(
    TAggregate        aggregate,
    TCommand          command,
    CancellationToken cancellationToken)
    where TAggregate : Aggregate;

public record RegisteredHandler<T>(
    ExpectedState ExpectedState,
    Func<T, object, CancellationToken, ValueTask<T>> Handler);

public class HandlersMap<TAggregate> : Dictionary<Type, RegisteredHandler<TAggregate>>
    where TAggregate : Aggregate
{
    public void AddHandler<TCommand>(RegisteredHandler<TAggregate> handler)
    {
        if (ContainsKey(typeof(TCommand)))
        {
            // TODO: log
            throw new CommandHandlerAlreadyRegistered<TCommand>();
        }

        Add(typeof(TCommand), handler);
    }

    public void AddHandler<TCommand>(ExpectedState expectedState, ActOnAggregateAsync<TAggregate, TCommand> action)
    {
        AddHandler<TCommand>(
            new RegisteredHandler<TAggregate>(
                expectedState,
                async (aggregate, cmd, ct) => {
                    await action(aggregate, (TCommand)cmd, ct).NoContext();
                    return aggregate;
                }));
    }

    public void AddHandler<TCommand>(ExpectedState expectedState, ActOnAggregate<TAggregate, TCommand> action) =>
        AddHandler<TCommand>(
            new RegisteredHandler<TAggregate>(
                expectedState,
                (aggregate, cmd, _) =>
                {
                    action(aggregate, (TCommand)cmd);
                    return new ValueTask<TAggregate>(aggregate); 
                }));
}