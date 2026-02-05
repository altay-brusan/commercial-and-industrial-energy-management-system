namespace Backend.Domain.Events;

public class FirmMarketOperatorDeletedEvent : BaseEvent
{
    public FirmMarketOperatorDeletedEvent(FirmMarketOperator item)
    {
        Item = item;
    }

    public FirmMarketOperator Item { get; }
}
