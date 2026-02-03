namespace Backend.Domain.Events;

public class TransmissionSystemOperatorDeletedEvent : BaseEvent
{
    public TransmissionSystemOperatorDeletedEvent(TransmissionSystemOperator item)
    {
        Item = item;
    }

    public TransmissionSystemOperator Item { get; }
}
