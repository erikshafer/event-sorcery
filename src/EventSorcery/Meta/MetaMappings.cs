using System.Diagnostics;

namespace EventSorcery.Meta;

public static class MetaMappings
{
    public static readonly IDictionary<string, string> TelemetryToInternalTagsMap = new Dictionary<string, string>
    {
        { TelemetryTags.Message.Id, MetaTags.MessageId },
        { TelemetryTags.Messaging.CausationId, MetaTags.CausationId },
        { TelemetryTags.Messaging.CorrelationId, MetaTags.CorrelationId }
    };

    public static readonly IDictionary<string, string> InternalToTelemetryTagsMap = new Dictionary<string, string>
    {
        { MetaTags.MessageId, TelemetryTags.Message.Id },
        { MetaTags.CausationId, TelemetryTags.Messaging.CausationId },
        { MetaTags.CorrelationId, TelemetryTags.Messaging.CorrelationId }
    };
}

public static class TelemetryTags
{
    public static class Message
    {
        public const string Type = "message.type";
        public const string Id = "message.id";
    }

    public static class Messaging
    {
        public const string MessageId = "messaging.message_id";
        public const string ConversationId = "messaging.conversation_id";
        public const string CorrelationId = "messaging.correlation_id";
        public const string CausationId = "messaging.causation_id";
    }
}

public static class DiagnosticTags
{
    public const string TraceId = "trace-id";
    public const string SpanId = "span-id";
    public const string ParentSpanId = "parent-span-id";
}

public record TracingMeta(string? TraceId, string? SpanId, string? ParentSpanId)
{
    private bool IsValid()
    {
        return TraceId != null && SpanId != null;
    }

    public ActivityContext? ToActivityContext(bool isRemote)
    {
        try
        {
            return IsValid()
                ? new ActivityContext(
                    ActivityTraceId.CreateFromString(TraceId),
                    ActivitySpanId.CreateFromString(SpanId),
                    ActivityTraceFlags.Recorded,
                    isRemote: isRemote)
                : default;
        }
        catch (Exception)
        {
            return default;
        }
    }
}