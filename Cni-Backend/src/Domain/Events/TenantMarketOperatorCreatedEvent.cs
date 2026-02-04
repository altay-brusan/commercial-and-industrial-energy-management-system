namespace Backend.Domain.Events;

public class TenantMarketOperatorCreatedEvent : BaseEvent
{
    public TenantMarketOperatorCreatedEvent(TenantMarketOperator item)
    {
        Item = item;
    }

    public TenantMarketOperator Item { get; }
}
