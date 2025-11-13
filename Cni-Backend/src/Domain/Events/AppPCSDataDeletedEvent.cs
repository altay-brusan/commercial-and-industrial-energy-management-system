using Backend.Domain.Entities.Proto;

namespace Backend.Domain.Events;

public class AppPCSDataDeletedEvent : BaseEvent
{
    public AppPCSDataDeletedEvent(AppPCSData item)
    {
        Item = item;
    }

    public AppPCSData Item { get; }
}
