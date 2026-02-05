using Backend.Domain.Entities.DeviceData;

namespace Backend.Domain.Events;

public class DevicePcsDataCreatedEvent : BaseEvent
{
    public DevicePcsDataCreatedEvent(DevicePcsData item)
    {
        Item = item;
    }

    public DevicePcsData Item { get; }
}
