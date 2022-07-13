using EventSorcery.Aggregates;

namespace EventSorcery.Applications;

public interface IApplicationService
{
    Task<Result> Handle(object command, CancellationToken cancellationToken);
}

public interface IApplicationService<T> : IApplicationService where T : Aggregate { }

public interface IApplicationService<TAggregate, TState, TId>
    where TAggregate : Aggregate<TState>
    where TState : AggregateState<TState>, new()
    where TId : AggregateId
{
    Task<Result<TState>> Handle(object command, CancellationToken cancellationToken);
}