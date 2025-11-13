using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities.Proto.DTO;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

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
