namespace EventSorcery.Exceptions;

public class InvalidStreamName : Exception {
    public InvalidStreamName(string? streamName)
        : base($"Stream name is {(string.IsNullOrWhiteSpace(streamName) ? "empty" : "invalid")}") { }
}