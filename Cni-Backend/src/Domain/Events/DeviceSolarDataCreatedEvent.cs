using Backend.Domain.Entities.DeviceData;

namespace Backend.Domain.Events;

public class DeviceSolarDataCreatedEvent : BaseEvent
{
    public DeviceSolarDataCreatedEvent(DeviceSolarData item)
    {
        Item = item;
    }

    public DeviceSolarData Item { get; }
}
