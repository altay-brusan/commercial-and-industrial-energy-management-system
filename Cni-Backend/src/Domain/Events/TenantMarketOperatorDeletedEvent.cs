namespace Backend.Domain.Events;

public class TenantMarketOperatorDeletedEvent : BaseEvent
{
    public TenantMarketOperatorDeletedEvent(TenantMarketOperator item)
    {
        Item = item;
    }

    public TenantMarketOperator Item { get; }
}
