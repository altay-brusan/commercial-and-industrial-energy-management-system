using Backend.Application.Scada.DTOs;

namespace Backend.Application.Common.Interfaces;

public interface IRealTimeNotifier
{
    Task SendFacilityUpdate(int facilityId, FacilityHubDto data);
}
