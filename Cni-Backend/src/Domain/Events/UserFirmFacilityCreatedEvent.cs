namespace Backend.Domain.Events;

public class UserFirmFacilityCreatedEvent : BaseEvent
{
    public UserFirmFacilityCreatedEvent(UserFirmFacility item)
    {
        Item = item;
    }

    public UserFirmFacility Item { get; }
}
