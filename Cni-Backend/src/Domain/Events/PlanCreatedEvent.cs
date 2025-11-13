namespace Backend.Domain.Events;

public class PlanCreatedEvent : BaseEvent
{
    public PlanCreatedEvent(Plan item)
    {
        Item = item;
    }

    public Plan Item { get; }
}
