namespace Backend.Domain.Events;

public class FirmCreatedEvent : BaseEvent
{
    public FirmCreatedEvent(Firm item)
    {
        Item = item;
    }

    public Firm Item { get; }
}
