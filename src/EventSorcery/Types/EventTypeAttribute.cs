namespace EventSorcery.Types;

[AttributeUsage(AttributeTargets.Class)]
public class EventTypeAttribute : Attribute
{
    public string EventType { get; }

    public EventTypeAttribute(string eventType) => EventType = eventType;
}