using Backend.Domain.Entities.DeviceData;

namespace Backend.Domain.Events;

public class DeviceBatteryDataCreatedEvent : BaseEvent
{
    public DeviceBatteryDataCreatedEvent(DeviceBatteryData item)
    {
        Item = item;
    }

    public DeviceBatteryData Item { get; }
}
