using Ems.Bess.Stream;
using Ems.Solar.Stream;
using Ems.Types.Generic;
using Grpc.Core;
using GrpcServer.Services.Helpers;
using System;
using System.Threading.Tasks;

namespace GrpcServer.Services.Streams
{
    public class SolarStreamService : PPCService.PPCServiceBase
    {
        private readonly ILogger<BesStreamService> _logger;
        private static readonly Random Random = new();


        public SolarStreamService(ILogger<BesStreamService> logger)
        {
            _logger = logger;
        }

        public override async Task StreamSolarData(SolarDataRequest request, IServerStreamWriter<SolarData> responseStream, ServerCallContext context)
        {
            var random = new Random();

            while (!context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Taking Solar datafor ID {request.Id} with {request.PeriodS} seconds period");

                var solarData = TakeSolarJsonData(request.Id);

                await responseStream.WriteAsync(solarData);
                await Task.Delay((int)request.PeriodS * 1000);

            }
        }
        private static SolarData TakeSolarJsonData(uint deviceId)
        {
            //def status tanımla hepsi actvive no warning
        
            var data = new SolarData
            {
                Id = deviceId,
                DCOutput=new DC_Power {
                    Power = StreamDataReader.GetValue<double>("stream_solar_dc_output_power"),
                    TotalEnergyDaily= StreamDataReader.GetValue<double>("stream_solar_dc_output_total_energy_daily"),
                },
                PanelPower= new DC_Power {
                    Power= StreamDataReader.GetValue<double>("stream_solar_panel_power_power"), 
                },
                PerformanceRatio= StreamDataReader.GetValue<double>("stream_solar_performance_ratio"),
                PvPanelTemperature= StreamDataReader.GetValue<double>("stream_solar_pv_panel_temperature"),
                Status=new Ems.Types.Generic.Status { WarningStatus= StreamDataReader.GetValue<bool>("stream_solar_status_warning_status"),OperatingMode=(OperatingMode)1,FaultStatus=false }
            };
            return data;
        }

       
    }
}
