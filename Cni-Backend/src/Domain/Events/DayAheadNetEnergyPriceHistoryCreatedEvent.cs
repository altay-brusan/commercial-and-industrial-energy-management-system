namespace Backend.Domain.Events;

public class DayAheadNetEnergyPriceHistoryCreatedEvent : BaseEvent
{
    public DayAheadNetEnergyPriceHistoryCreatedEvent(DayAheadNetEnergyPriceHistory item)
    {
        Item = item;
    }

    public DayAheadNetEnergyPriceHistory Item { get; }
}
