using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class AppBESSDataCreatedEvent : BaseEvent
{
    public AppBESSDataCreatedEvent(AppBESSData item)
    {
        Item = item;
    }

    public AppBESSData Item { get; }
}
