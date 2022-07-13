using System.Text.Json.Serialization;
using EventSorcery.Aggregates;
using EventSorcery.Tools;

namespace EventSorcery.Applications;

public record struct Change(object Event, string EventType);

public abstract record Result(object? State, bool Success, IEnumerable<Change>? Changes = null);

public record OkResult(object State, IEnumerable<Change>? Changes = null) : Result(State, true, Changes);

public record ErrorResult : Result
{
    public ErrorResult(string message, Exception? exception) : base(null, false) {
        Message   = message;
        Exception = exception;
    }

    [JsonIgnore]
    public Exception? Exception { get; }

    public string ErrorMessage => Exception?.Message ?? "Unknown error";

    public string Message { get; }
}

public abstract record Result<TState>(TState? State, bool Success, IEnumerable<Change>? Changes = null)
    where TState : AggregateState<TState>, new();

public record OkResult<TState>(TState State, IEnumerable<Change> Changes, ulong StreamPosition)
    : Result<TState>(State, true, Changes)
    where TState : AggregateState<TState>, new();

public record ErrorResult<TState> : Result<TState>
    where TState : AggregateState<TState>, new()
{
    public string Message { get; init; }

    [JsonIgnore]
    public Exception? Exception { get; init; }

    public string? ErrorMessage => Exception?.Message;
    
    public ErrorResult(string message, Exception? exception) : base(null, false)
    {
        Message   = message;
        Exception = exception;
    }

    public ErrorResult(Exception exception) : base(null, false)
    {
        Exception = Ensure.NotNull(exception);
        Message   = exception.Message;
    }
}