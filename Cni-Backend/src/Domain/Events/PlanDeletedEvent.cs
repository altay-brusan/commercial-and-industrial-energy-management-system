namespace Backend.Domain.Events;

public class PlanDeletedEvent : BaseEvent
{
    public PlanDeletedEvent(Plan item)
    {
        Item = item;
    }

    public Plan Item { get; }
}
