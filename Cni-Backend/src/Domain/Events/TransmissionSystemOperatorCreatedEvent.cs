namespace Backend.Domain.Events;

public class TransmissionSystemOperatorCreatedEvent : BaseEvent
{
    public TransmissionSystemOperatorCreatedEvent(TransmissionSystemOperator item)
    {
        Item = item;
    }

    public TransmissionSystemOperator Item { get; }
}
