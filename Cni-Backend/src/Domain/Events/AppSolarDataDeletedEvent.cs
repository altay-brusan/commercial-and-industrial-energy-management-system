using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class AppSolarDataDeletedEvent : BaseEvent
{
    public AppSolarDataDeletedEvent(AppSolarData item)
    {
        Item = item;
    }

    public AppSolarData Item { get; }
}
