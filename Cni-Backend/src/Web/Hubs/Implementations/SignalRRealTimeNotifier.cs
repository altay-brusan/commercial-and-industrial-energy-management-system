using Backend.Application.Common.Interfaces;
using Backend.Application.Scada.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Web.Hubs.Implementations;

public class SignalRRealTimeNotifier : IRealTimeNotifier
{
    private readonly IHubContext<LiveDataHub> _hubContext;

    public SignalRRealTimeNotifier(IHubContext<LiveDataHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendFacilityUpdate(int facilityId, FacilityHubDto data)
    {
        await _hubContext.Clients
            .Group($"facility-{facilityId}")
            .SendAsync("FacilityUpdate", data);
    }
}
