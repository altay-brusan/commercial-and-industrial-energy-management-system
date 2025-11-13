using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class AppSolarDataCreatedEvent : BaseEvent
{
    public AppSolarDataCreatedEvent(AppSolarData item)
    {
        Item = item;
    }

    public AppSolarData Item { get; }
}
