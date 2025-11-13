using Ems.System.Command;
using Grpc.Core;

namespace GrpcServer.Services.Commands
{
    public class SystemSetupCommandService : SystemSetupService.SystemSetupServiceBase
    {
        private readonly ILogger<SystemSetupCommandService> _logger;

        public SystemSetupCommandService(ILogger<SystemSetupCommandService> logger)
        {
            _logger = logger;
        }

        //public override Task<CommandResponseData> SetBESSId(IdArrayData request, ServerCallContext context)
        //{
        //    _logger.LogInformation("Setting BESS Ids...");
        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = "BESS Ids set successfully.",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}

        //public override Task<CommandResponseData> SetPCSId(IdArrayData request, ServerCallContext context)
        //{
        //    _logger.LogInformation("Setting PCS Ids...");
        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = "PCS Ids set successfully.",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}

        //public override Task<CommandResponseData> SetSmartMeterId(IdArrayData request, ServerCallContext context)
        //{
        //    _logger.LogInformation("Setting Smart Meter Ids...");
        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = "Smart Meter Ids set successfully.",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}

        //public override Task<CommandResponseData> SetSolarId(IdArrayData request, ServerCallContext context)
        //{
        //    _logger.LogInformation("Setting Solar Ids...");
        //    return Task.FromResult(new CommandResponseData
        //    {
        //        Success = true,
        //        Message = "Solar Ids set successfully.",
        //        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        //    });
        //}
    }
}
