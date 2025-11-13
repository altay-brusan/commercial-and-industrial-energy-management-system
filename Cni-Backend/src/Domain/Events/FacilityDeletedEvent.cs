namespace Backend.Domain.Events;

public class FacilityDeletedEvent : BaseEvent
{
    public FacilityDeletedEvent(Facility item)
    {
        Item = item;
    }

    public Facility Item { get; }
}
