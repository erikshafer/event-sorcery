using EventSorcery.Meta;

namespace EventSorcery.Stores; 

public record StreamEvent(
    Guid Id,
    object? Payload,
    Metadata Metadata,
    string ContentType,
    long Position);
