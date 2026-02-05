namespace Backend.Domain.Events;

public class MarketOperatorCreatedEvent : BaseEvent
{
    public MarketOperatorCreatedEvent(MarketOperator item)
    {
        Item = item;
    }

    public MarketOperator Item { get; }
}
