using Backend.Domain.Entities;

namespace Backend.Domain.Events;

public class DeviceDeletedEvent : BaseEvent
{
    public DeviceDeletedEvent(Device item)
    {
        Item = item;
    }

    public Device Item { get; }
}
