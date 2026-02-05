namespace Backend.Domain.Events;

public class MarketOperatorDeletedEvent : BaseEvent
{
    public MarketOperatorDeletedEvent(MarketOperator item)
    {
        Item = item;
    }

    public MarketOperator Item { get; }
}
