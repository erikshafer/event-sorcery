namespace EventSorcery.Meta;

public static class MetaTags
{
    private const string Prefix = "eventuous";

    public const string MessageId = $"{Prefix}.message-id";
    public const string CorrelationId = $"{Prefix}.correlation-id";
    public const string CausationId = $"{Prefix}.causation-id";
}