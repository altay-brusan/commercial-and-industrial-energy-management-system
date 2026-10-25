using Ems.System.Command;
using Grpc.Core;
using System.Collections.Concurrent;

namespace GrpcServer.Services.Commands
{
    public class SystemPowerCommandService : SystemPowerService.SystemPowerServiceBase
    {
        private readonly ILogger<SystemPowerCommandService> _logger;

        private static readonly ConcurrentDictionary<string, SystemModeData> SystemModes = new();

        static SystemPowerCommandService()
        {
            var defaultData = new SystemModeData
            {
                Id = "1234",
                Mode = BESSMode.BessStandby
            };
            SystemModes.TryAdd(defaultData.Id, defaultData);
        }

        public SystemPowerCommandService(ILogger<SystemPowerCommandService> logger)
        {
            _logger = logger;
        }

        //public override Task<CommandResponseData> SetSystemMode(SystemModeData request, ServerCallContext context)
        //{
        //    _logger.LogInformation($"System mode set to: {request.Mode}");
        //    SystemModes[request.Id] = new SystemModeData
        //    {
        //        Id = request.Id,
        //        Mode = request.Mode
        //    };

        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = $"Mode changed to {request.Mode}",
        //    });
        //}

        //public override Task<SystemModeData> GetSystemMode(Empty request, ServerCallContext context)
        //{
        //    var data = SystemModes.Values.FirstOrDefault() ?? new SystemModeData
        //    {
        //        Id = "1234",
        //        Mode = BESSMode.BessStandby
        //    };

        //    return Task.FromResult(data);
        //}

        //public override Task<CommandResponseData> SetChargePower(ChargePowerData request, ServerCallContext context)
        //{
        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = $"Charge power set to {request.Power} kW",
        //    });
        //}

        //public override Task<CommandResponseData> SetDischargePower(DischargePowerData request, ServerCallContext context)
        //{
        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = $"Discharge power set to {request.Power} kW",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}

        //public override Task<CommandResponseData> EmergencyShutDown(Empty request, ServerCallContext context)
        //{
        //    _logger.LogWarning("Emergency shutdown initiated!");

        //    foreach (var key in SystemModes.Keys)
        //    {
        //        SystemModes[key] = new SystemModeData
        //        {
        //            Id = key,
        //            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        //            Mode = BESSMode.BessStandby
        //        };
        //    }

        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = "Emergency shutdown completed. System is now in Standby mode.",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}
    }
}
