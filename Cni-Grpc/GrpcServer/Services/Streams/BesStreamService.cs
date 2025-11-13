using System.Collections.Concurrent;
using Ems.Bess.Stream;
using Ems.Types.Generic;
using Grpc.Core;
using GrpcServer.Services.Helpers;

namespace GrpcServer.Services.Streams
{
    public class BesStreamService : BESSService.BESSServiceBase
    {
        private readonly ILogger<BesStreamService> _logger;
        private static readonly Random Random = new();


        public BesStreamService(ILogger<BesStreamService> logger)
        {
            _logger = logger;
        }

        public override async Task StreamBESSData(BESSDataRequest request, IServerStreamWriter<BESSData> responseStream, ServerCallContext context)
        {

            while (!context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Taking Bess datafor ID {request.Id} with {request.PeriodS} seconds period");
                var data = TakeBessJsonData(request.Id);
                await responseStream.WriteAsync(data);
                await Task.Delay(TimeSpan.FromSeconds(request.PeriodS), context.CancellationToken);
            }
        }

        private static BESSData TakeBessJsonData(uint deviceId)
        {
            var dc_powerValue = StreamDataReader.GetValue<double>("stream_bess_dc_power_power");
            var charge_statusValue= StreamDataReader.GetValue<int>("stream_bess_charge_status");
            var status = new Ems.Types.Generic.Status
            {
                OperatingMode = (OperatingMode)1,
                WarningStatus = false,  // 20% olasılıkla uyarı
                FaultStatus = false      // 10% olasılıkla hata
            };

            var data = new BESSData
            {
                Id=deviceId,
                StateOfCharge = StreamDataReader.GetValue<double>("stream_bess_state_of_charge"),
                StateOfHealth = StreamDataReader.GetValue<double>("stream_bess_state_of_health"),
                DcPower = new DC_Power { Current = 0, Power = dc_powerValue, Voltage = 0 },
                BatteryTemperature = StreamDataReader.GetValue<double>("stream_bess_battery_temperature"),
                TotalEnergyStored = StreamDataReader.GetValue<double>("stream_bess_total_energy_stored"),
                CoolingActive= StreamDataReader.GetValue<bool>("stream_bess_cooling_active"),
                Capacity = StreamDataReader.GetValue<double>("stream_bess_capacity"),
                ChargeStatus=(BESSStatus)charge_statusValue,
                Status=status
            };
            return data;
        }
    
        private static OperatingMode RandomOperatingMode()
        {
            var values = Enum.GetValues(typeof(OperatingMode));
            return (OperatingMode)values.GetValue(Random.Next(values.Length))!;
        }
    }
}