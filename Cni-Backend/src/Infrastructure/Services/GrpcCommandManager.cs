using Ems.System.Command;
using Grpc.Net.Client;
using Empty = Ems.System.Command.Empty;

namespace Backend.Infrastructure.Services;
public class GrpcCommandManager
{
    private readonly string _grpcServerAddress;

    public GrpcCommandManager(string grpcServerAddress)
    {
        _grpcServerAddress = grpcServerAddress;
    }

    public async Task<CommandResponse> SetSystemModeAsync(SystemModeData request)
    {
        using var channel = GrpcChannel.ForAddress(_grpcServerAddress);
        var client = new SystemModeService.SystemModeServiceClient(channel);
        return await client.SetSystemModeAsync(request);
    }

    public async Task<SystemModeData> GetSystemModeAsync()
    {
        using var channel = GrpcChannel.ForAddress(_grpcServerAddress);
        var client = new SystemModeService.SystemModeServiceClient(channel);
        return await client.GetSystemModeAsync(new Empty());
    }

    public async Task<CommandResponse> SetChargePowerAsync(float chargePower)
    {
        using var channel = GrpcChannel.ForAddress(_grpcServerAddress);
        var client = new SystemModeService.SystemModeServiceClient(channel);
        var request = new ChargePowerData { ChargePower = chargePower };
        return await client.SetChargePowerAsync(request);
    }

    public async Task<CommandResponse> SetDischargePowerAsync(float totalPower, float powerFactor)
    {
        using var channel = GrpcChannel.ForAddress(_grpcServerAddress);
        var client = new SystemModeService.SystemModeServiceClient(channel);
        var request = new DischargePowerData { TotalPower = totalPower, PowerFactor = powerFactor };
        return await client.SetDischargePowerAsync(request);
    }

    public async Task<CommandResponse> EmergencyShutDownAsync()
    {
        using var channel = GrpcChannel.ForAddress(_grpcServerAddress);
        var client = new SystemModeService.SystemModeServiceClient(channel);
        return await client.EmergencyShutDownAsync(new Empty());
    }
}
