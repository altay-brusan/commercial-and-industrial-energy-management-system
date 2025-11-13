using Microsoft.Extensions.Logging;
using Backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Backend.Infrastructure.Services;

namespace Backend.Infrastructure.BackgroundJobs
{
    public class GrpcFacilityWorker
    {
        private readonly ILogger<GrpcFacilityWorker> _logger;
        private readonly IApplicationDbContext _context;
        private readonly GrpcStreamManager _grpcService;

        public GrpcFacilityWorker(ILogger<GrpcFacilityWorker> logger, IApplicationDbContext context, GrpcStreamManager grpcService)
        {
            _logger = logger;
            _context = context;
            _grpcService = grpcService;
        }

        public async Task CheckFacilities()
        {
            _logger.LogInformation("Facility listesi çekiliyor...");
            try
            {
                //eğer timer null değil ise timerı ı date.now a göre set et;
                var facilities = await _context.Facilities.IgnoreQueryFilters().Where(x => x.GrpcStatus == true).ToListAsync();


                foreach (var facility in facilities)
                {
                    var devices = await _context.Devices
                        .Include(d => d.TopologyNode)
                        .Include(d => d.DeviceType)
                        .Include(d => d.ConnectedDevice)
                            .ThenInclude(cd => cd!.DeviceType)
                        .Include(d => d.ConnectedDevice)
                            .ThenInclude(cd => cd!.TopologyNode)
                        .IgnoreQueryFilters()
                        .Where(d => d.FacilityId == facility.Id && d.GrpcConnection)
                        .ToListAsync();
                    if (!devices.Any())
                    {
                        _logger.LogWarning($"[Facility {facility.Id}] için cihaz bulunamadı!");
                        continue;
                    }

                    // Tüm cihaz tasklarının çalışıp çalışmadığını kontrol et
                    var isAnyDeviceRunning = _grpcService.IsFacilityRunning(facility.Id);

                    if (isAnyDeviceRunning)
                    {
                        _logger.LogInformation($"[Facility {facility.Id}] için streamler zaten çalışıyor.");
                    }
                    else
                    {
                        _logger.LogInformation($"[Facility {facility.Id}] için yeni streamler başlatılıyor...");
                        _grpcService.StartFacilityStreams(facility, devices);
                        continue;
                    }

                    var trackedDeviceIds = _grpcService.GetTrackedDeviceIds(facility.Id);

                    if (trackedDeviceIds != null)
                    {
                        var newDevices = devices
                            .Where(d => !trackedDeviceIds.Contains(d.Id))
                            .ToList();

                        var removedDeviceIds = trackedDeviceIds
                            .Except(devices.Select(d => d.Id))
                            .ToList();

                        // Yeni cihazları ekle
                        foreach (var device in newDevices)
                        {
                            _grpcService.AddDeviceToFacility(facility, device);
                            _logger.LogInformation($"[Facility {facility.Id}] Yeni cihaz eklendi: {device.Id}");
                        }

                        // Kaldırılan cihazları sil
                        foreach (var deviceId in removedDeviceIds)
                        {
                            _grpcService.RemoveDeviceFromFacility(facility.Id, deviceId);
                            _logger.LogInformation($"[Facility {facility.Id}] Cihaz kaldırıldı: {deviceId}");
                        }
                    }

                }

                _logger.LogInformation("Facility listesi kontrolü tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }
    }
}
