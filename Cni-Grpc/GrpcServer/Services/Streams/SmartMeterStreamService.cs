using System;
using System.Threading.Tasks;
using Grpc.Core;
using Ems.Smartmeter.Stream;
using Ems.Types.Generic;
using Microsoft.Extensions.Logging;
using Ems.Solar.Stream;
using GrpcServer.Services.Helpers;

namespace GrpcServer.Services.Streams
{
    public class SmartMeterStreamService : SmartMeterService.SmartMeterServiceBase
    {
        private readonly ILogger<SmartMeterStreamService> _logger;
        private static readonly Random Random = new();

        // Örnek uyarı/hata mesajları
        private static readonly List<string> WarningMessages = new()
        {
            "Voltage surge detected",
            "Phase imbalance",
            "Low power factor"
        };

        private static readonly List<string> FaultMessages = new()
        {
            "Meter tamper detected",
            "Phase loss",
            "Overcurrent fault"
        };

        public SmartMeterStreamService(ILogger<SmartMeterStreamService> logger)
        {
            _logger = logger;
        }

        public override async Task StreamSmartMeterData(SmartMeterDataRequest request, IServerStreamWriter<SmartMeterData> responseStream, ServerCallContext context)
        {

            while (!context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Taking Smartmeter data for ID {request.Id} with {request.PeriodS} seconds period");

                var data = TakeSmartmeterJsonData(request.Id);
                await responseStream.WriteAsync(data);
                await Task.Delay(TimeSpan.FromSeconds(request.PeriodS), context.CancellationToken);
            }
        }


        private static SmartMeterData TakeSmartmeterJsonData(uint deviceId)
        {
            var status = new Ems.Types.Generic.Status
            {
                OperatingMode = (OperatingMode)1,
                WarningStatus = false,  // 20% olasılıkla uyarı
                FaultStatus = false      // 10% olasılıkla hata
            };
            var data = new SmartMeterData();
            if (deviceId == 5)
            {
                //factory
                data = new SmartMeterData
                {
                    AcPower = new AC_Power
                    {
                        TotalReactivePower = StreamDataReader.GetValue<double>("stream_smartmeter_fac_ac_power_total_energy_reactive"),
                        TotalPowerFactor = StreamDataReader.GetValue<double>("stream_smartmeter_fac_ac_power_power_factor"),
                        TotalActivePower = StreamDataReader.GetValue<double>("stream_smartmeter_fac_ac_power_total_energy_active"),
                        TotalEnergyDaily = StreamDataReader.GetValue<double>("stream_smartmeter_fac_ac_power_total_energy_daily"),
                    },
                    Status= status
                    
                };
            }
            else if (deviceId == 4)
            {
                data = new SmartMeterData
                {
                    AcPower = new AC_Power
                    {
                        TotalReactivePower = StreamDataReader.GetValue<double>("stream_smartmeter_grid_ac_power_total_energy_reactive"),
                        TotalPowerFactor = StreamDataReader.GetValue<double>("stream_smartmeter_grid_ac_power_power_factor"),
                        TotalActivePower = StreamDataReader.GetValue<double>("stream_smartmeter_grid_ac_power_total_energy_active"),
                    },
                    Status = status
                };
            }
            return data;
        }


        private static OperatingMode RandomOperatingMode()
        {
            var values = Enum.GetValues(typeof(OperatingMode));
            return (OperatingMode)values.GetValue(Random.Next(values.Length))!;
        }
    }
}