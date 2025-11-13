using Microsoft.AspNetCore.SignalR;
using Backend.Infrastructure.Services; // GrpcStreamManager'a erişim için
using Backend.Application.Common.Interfaces;

namespace Backend.Web.Hubs;

public class LiveDataHub : Hub
{
    private readonly GrpcStreamManager _grpcStreamManager;

    public LiveDataHub(GrpcStreamManager grpcStreamManager)
    {
        _grpcStreamManager = grpcStreamManager;
    }

    public async Task SubscribeToFacility(int facilityId)
    {
        // Gruba ekle
        await Groups.AddToGroupAsync(Context.ConnectionId, $"facility-{facilityId}");

        // Anlık veriyi gönder
        if (_grpcStreamManager._faciltyHubEntites.TryGetValue(facilityId, out var data))
        {
            await Clients.Caller.SendAsync("FacilityUpdate", data);
        }
    }
}
