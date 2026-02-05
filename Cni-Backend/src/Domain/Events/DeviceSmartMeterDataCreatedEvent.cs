using Backend.Domain.Entities.DeviceData;

namespace Backend.Domain.Events;

public class DeviceSmartMeterDataCreatedEvent : BaseEvent
{
    public DeviceSmartMeterDataCreatedEvent(DeviceSmartMeterData item)
    {
        Item = item;
    }

    public DeviceSmartMeterData Item { get; }
}
