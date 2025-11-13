using Ems.Solar.Stream;
using Ems.System.Stream;
using Ems.Types.Generic;
using Grpc.Core;
using GrpcServer.Services.Helpers;

namespace GrpcServer.Services.Streams
{
    public class SystemStreamService : SystemService.SystemServiceBase
    {
        private readonly ILogger<BesStreamService> _logger;


        public SystemStreamService(ILogger<BesStreamService> logger)
        {
            _logger = logger;
        }

        public override async Task StreamSystemData(SystemDataRequest request, IServerStreamWriter<SystemData> responseStream, ServerCallContext context)
        {
            _logger.LogInformation($"Starting System data stream for Facility ID {request.Id} /with {request.PeriodS} seconds period");

            while (!context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Taking System data stream Facility for ID {request.Id} /with {request.PeriodS} seconds period");

                var solarData = TakeSystemJsonData(request.Id);

                await responseStream.WriteAsync(solarData);
                await Task.Delay((int)request.PeriodS * 1000);

            }
        }
        private static SystemData TakeSystemJsonData(uint deviceId)
        {

            var data = new SystemData
            {
                GeneratorPower = StreamDataReader.GetValue<double>("stream_system_generator_power"),
                GridPower= StreamDataReader.GetValue<double>("stream_system_grid_power"),
                LoadGeneratorContribution= StreamDataReader.GetValue<double>("stream_system_load_generator_contribution"),
                LoadGridContribution= StreamDataReader.GetValue<double>("stream_system_load_grid_contribution"),
                LoadPcsContribution= StreamDataReader.GetValue<double>("stream_system_load_pcs_contribution"),
                LoadSolarContribution= StreamDataReader.GetValue<double>("stream_system_load_solar_contribution"),
                PcsPower= StreamDataReader.GetValue<double>("stream_system_pcs_power"),
                SolarPower= StreamDataReader.GetValue<double>("stream_system_solar_power"),
            };
            return data;
        }


    }
}
