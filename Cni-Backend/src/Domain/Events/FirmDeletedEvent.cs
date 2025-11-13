namespace Backend.Domain.Events;

public class FirmDeletedEvent : BaseEvent
{
    public FirmDeletedEvent(Firm item)
    {
        Item = item;
    }

    public Firm Item { get; }
}
