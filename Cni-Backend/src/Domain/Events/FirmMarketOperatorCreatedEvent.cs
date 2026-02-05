namespace Backend.Domain.Events;

public class FirmMarketOperatorCreatedEvent : BaseEvent
{
    public FirmMarketOperatorCreatedEvent(FirmMarketOperator item)
    {
        Item = item;
    }

    public FirmMarketOperator Item { get; }
}
