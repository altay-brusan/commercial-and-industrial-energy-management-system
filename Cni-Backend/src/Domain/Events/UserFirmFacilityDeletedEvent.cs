namespace Backend.Domain.Events;

public class UserFirmFacilityDeletedEvent : BaseEvent
{
    public UserFirmFacilityDeletedEvent(UserFirmFacility item)
    {
        Item = item;
    }

    public UserFirmFacility Item { get; }
}
