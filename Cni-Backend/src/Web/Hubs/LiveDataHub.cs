using Microsoft.AspNetCore.SignalR;
using Backend.Application.Common.Interfaces;

namespace Backend.Web.Hubs;

public class LiveDataHub : Hub
{
    // gRPC disabled - LiveDataHub is now a no-op
    public async Task SubscribeToFacility(int facilityId)
    {
        // Gruba ekle
        await Groups.AddToGroupAsync(Context.ConnectionId, $"facility-{facilityId}");

        // gRPC disabled - no live data available
    }
}
