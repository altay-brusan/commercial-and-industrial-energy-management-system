namespace Backend.Domain.Events;

public class TenantDeletedEvent : BaseEvent
{
    public TenantDeletedEvent(Tenant item)
    {
        Item = item;
    }

    public Tenant Item { get; }
}
