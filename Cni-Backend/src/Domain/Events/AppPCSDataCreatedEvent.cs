using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class AppPCSDataCreatedEvent : BaseEvent
{
    public AppPCSDataCreatedEvent(AppPCSData item)
    {
        Item = item;
    }

    public AppPCSData Item { get; }
}
