using Ems.Bess.Stream;
using Ems.Pcs.Stream;
using Ems.Solar.Stream;
using Ems.Types.Generic;
using Grpc.Core;
using GrpcServer.Services.Helpers;
using System;
using System.Threading.Tasks;

namespace GrpcServer.Services.Streams
{
    public class PCStreamService : PCService.PCServiceBase
    {
        private readonly ILogger<BesStreamService> _logger;
        private static readonly Random Random = new();


        public PCStreamService(ILogger<BesStreamService> logger)
        {
            _logger = logger;
        }
        public override async Task StreamPCSData(PCSDataRequest request, IServerStreamWriter<PCSData> responseStream, ServerCallContext context)
        {

            while (!context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Starting PCS stream for ID {request.Id} (Update interval: {request.PeriodS}s)");

                var data = TakePCSJsonData(request.Id);

                await responseStream.WriteAsync(data);

                await Task.Delay(TimeSpan.FromSeconds(request.PeriodS), context.CancellationToken);
            }
        }
        private static PCSData TakePCSJsonData(uint deviceId)
        {
            var dc_powerValue = StreamDataReader.GetValue<double>("stream_bess_dc_power_power");
            var charge_statusValue = StreamDataReader.GetValue<int>("stream_bess_charge_status");
            var status = new Ems.Types.Generic.Status
            {
                OperatingMode = (OperatingMode)1,
                WarningStatus = false,  // 20% olasılıkla uyarı
                FaultStatus = false      // 10% olasılıkla hata
            };
            var data = new PCSData
            {
                Id = deviceId,
                Efficiency = StreamDataReader.GetValue<double>("stream_pcs_efficiency"),
                TemperaturePcs = StreamDataReader.GetValue<double>("stream_pcs_pcs_temperature"),
                Status = status,
                AcPower = new AC_Power
                {
                    TotalActivePower = StreamDataReader.GetValue<double>("stream_pcs_ac_power_total_energy_active"),
                    CurrentRms= StreamDataReader.GetValue<double>("stream_pcs_ac_power_current_rms"),
                    VoltageRms = StreamDataReader.GetValue<double>("stream_pcs_ac_power_voltage_rms")
                },
                DcPower=new DC_Power
                {
                    Current= StreamDataReader.GetValue<double>("stream_pcs_dc_power_current"),
                    Voltage= StreamDataReader.GetValue<double>("stream_pcs_dc_power_voltage"),
                    Power= StreamDataReader.GetValue<double>("stream_pcs_dc_power_power"),
                }
                
            };
            return data;
        }
    }
}
