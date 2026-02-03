namespace Backend.Domain.Events;

public class EnergyPriceHistoryCreatedEvent : BaseEvent
{
    public EnergyPriceHistoryCreatedEvent(EnergyPriceHistory item)
    {
        Item = item;
    }

    public EnergyPriceHistory Item { get; }
}
