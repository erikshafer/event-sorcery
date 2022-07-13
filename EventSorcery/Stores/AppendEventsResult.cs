namespace EventSorcery.Stores;

public record AppendEventsResult(ulong GlobalPosition, long NextExpectedVersion)
{
    public static readonly AppendEventsResult NoOp =
        new(GlobalPosition: 0, NextExpectedVersion: -1);
}