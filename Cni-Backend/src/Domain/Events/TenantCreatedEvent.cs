namespace Backend.Domain.Events;

public class TenantCreatedEvent : BaseEvent
{
    public TenantCreatedEvent(Tenant item)
    {
        Item = item;
    }

    public Tenant Item { get; }
}
