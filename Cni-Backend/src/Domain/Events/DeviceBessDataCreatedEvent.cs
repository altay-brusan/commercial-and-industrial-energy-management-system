using Backend.Domain.Entities.DeviceData;

namespace Backend.Domain.Events;

public class DeviceBessDataCreatedEvent : BaseEvent
{
    public DeviceBessDataCreatedEvent(DeviceBessData item)
    {
        Item = item;
    }

    public DeviceBessData Item { get; }
}
