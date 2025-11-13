using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class AppBESSDataDeletedEvent : BaseEvent
{
    public AppBESSDataDeletedEvent(AppBESSData item)
    {
        Item = item;
    }

    public AppBESSData Item { get; }
}
