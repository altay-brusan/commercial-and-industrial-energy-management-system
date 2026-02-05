namespace Backend.Domain.Events;

public class DayAheadEnergyPriceHistoryCreatedEvent : BaseEvent
{
    public DayAheadEnergyPriceHistoryCreatedEvent(DayAheadEnergyPriceHistory item)
    {
        Item = item;
    }

    public DayAheadEnergyPriceHistory Item { get; }
}
