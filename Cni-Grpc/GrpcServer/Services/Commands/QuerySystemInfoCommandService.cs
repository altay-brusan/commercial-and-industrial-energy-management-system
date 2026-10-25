using Ems.System.Command;
using Grpc.Core;

namespace GrpcServer.Services.Commands
{
    public class QuerySystemInfoCommandService : QuerySystemInfoService.QuerySystemInfoServiceBase
    {
        //public override Task<SystemInfoData> GetSystemInfo(Empty request, ServerCallContext context)
        //{
        //    return Task.FromResult(new SystemInfoData
        //    {
        //        Id = "system_01",
        //        Name = "PowerSystem",
        //        Version = "1.0.0",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}

        //public override Task<SystemStateData> GetSystemState(Empty request, ServerCallContext context)
        //{
        //    return Task.FromResult(new SystemStateData
        //    {
        //        Id = "state_01",
        //        Mode = BESSMode.BessStandby,
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        //        Description = "System is currently in Standby mode."
        //    });
        //}
    }
}
