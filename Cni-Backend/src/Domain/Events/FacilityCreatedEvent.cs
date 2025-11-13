namespace Backend.Domain.Events;

public class FacilityCreatedEvent : BaseEvent
{
    public FacilityCreatedEvent(Facility item)
    {
        Item = item;
    }

    public Facility Item { get; }
}
