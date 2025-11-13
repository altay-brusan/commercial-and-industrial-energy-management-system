using System.Collections.Concurrent;
using AutoMapper;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Data;
using Ems.Bess.Stream;
using Ems.Solar.Stream;
using Ems.Pcs.Stream;
using Ems.Smartmeter.Stream;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Devices.Queries;
using Backend.Domain.Entities.Proto.DTO;
using Backend.Domain.Entities.Proto;
using Backend.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Services;

public class GrpcStreamManager
{
    public readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Task>> _facilityDeviceTasks = new();
    public readonly ConcurrentDictionary<int, FacilityHubDto> _faciltyHubEntites = new();
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, CancellationTokenSource>> _cancellationTokens = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IExternalDataService _externalDataService;
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _debounceTokens = new();
    private readonly ILogger<GrpcStreamManager> _logger;
    //public static Timer timer = new Timer();
    public GrpcStreamManager(IServiceScopeFactory scopeFactory, IMapper mapper, IRealTimeNotifier realTimeNotifier, ILogger<GrpcStreamManager> logger,IExternalDataService externalDataService)
    {
        _realTimeNotifier = realTimeNotifier;
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _logger = logger;
        _externalDataService = externalDataService;
    }

    private readonly ConcurrentDictionary<int, DateTime> _lastSentTimes = new();

    private async Task ScheduleDebouncedUpdate(int facilityId)
    {
        var now = DateTime.UtcNow;
        var newCts = new CancellationTokenSource();

        var oldCts = _debounceTokens.AddOrUpdate(facilityId,
            newCts,
            (key, existingCts) =>
            {
                existingCts.Cancel();
                existingCts.Dispose();
                return newCts;
            });

        try
        {
            // Minimum 1 saniye bekleyerek throttle uygula
            if (_lastSentTimes.TryGetValue(facilityId, out var lastSent) &&
                (now - lastSent).TotalSeconds < 1)
            {
                await Task.Delay(1000 - (int)(now - lastSent).TotalMilliseconds, newCts.Token);
            }
            else
            {
                await Task.Delay(200, newCts.Token); // Normal debounce
            }

            if (_faciltyHubEntites.TryGetValue(facilityId, out var hubDto))
            {
                await _realTimeNotifier.SendFacilityUpdate(facilityId, hubDto);
                _lastSentTimes[facilityId] = DateTime.UtcNow;
            }
        }
        catch (TaskCanceledException) { /* İptal edildi */ }
        finally
        {
            if (_debounceTokens.TryRemove(facilityId, out var removedCts) && removedCts == newCts)
            {
                newCts.Dispose();
            }
        }
    }

    public async void StartFacilityStreams(Facility facility, List<Device> devices)
    {
        StopFacilityStreams(facility.Id);

        var deviceTasks = new ConcurrentDictionary<int, Task>();
        var tokenSources = new ConcurrentDictionary<int, CancellationTokenSource>();
        var facilityHub = new FacilityHubDto();

        foreach (var device in devices)
        {
            await Task.Delay(6000);
            var cts = new CancellationTokenSource();
            tokenSources[device.Id] = cts;

            // Cihazı başlat
            deviceTasks[device.Id] = Task.Run(() =>
                RunDeviceStreamAsync(facility, device, cts.Token)
            );
        }

        _faciltyHubEntites[facility.Id] = facilityHub;
        _facilityDeviceTasks[facility.Id] = deviceTasks;
        _cancellationTokens[facility.Id] = tokenSources;
    }
    public bool IsFacilityRunning(int facilityId)
    {
        if (_facilityDeviceTasks.TryGetValue(facilityId, out var deviceTasks))
        {
            // En az bir aktif task varsa true dön
            return deviceTasks.Values.Any(t =>
                t != null &&
                !t.IsCompleted &&
                !t.IsFaulted &&
                !t.IsCanceled
            );
        }
        return false;
    }


    public void AddDeviceToFacility(Facility facility, Device newDevice)
    {
        if (!_facilityDeviceTasks.TryGetValue(facility.Id, out var deviceTasks))
        {
            throw new InvalidOperationException($"Facility {facility.Id} is not running!");
        }

        var cts = new CancellationTokenSource();
        var tokenSources = _cancellationTokens[facility.Id];

        if (!tokenSources.TryAdd(newDevice.Id, cts))
        {
            cts.Dispose();
            return;
        }

        var newTask = Task.Run(() => RunDeviceStreamAsync(
          facility,
            newDevice,
            cts.Token
        ));

        deviceTasks.TryAdd(newDevice.Id, newTask);
    }
    public void RemoveDeviceFromFacility(int facilityId, int deviceId)
    {
        if (_cancellationTokens.TryGetValue(facilityId, out var tokenSources) &&
            tokenSources.TryRemove(deviceId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        if (_facilityDeviceTasks.TryGetValue(facilityId, out var tasks))
        {
            tasks.TryRemove(deviceId, out _);
        }
    }
    public List<int>? GetTrackedDeviceIds(int facilityId)
    {
        if (_facilityDeviceTasks.TryGetValue(facilityId, out var deviceTasks))
        {
            var activeDevices = deviceTasks.Keys.ToList();
            // En az bir aktif task varsa true dön
            return activeDevices;
        }

        return null;

    }

    private async Task RunDeviceStreamAsync(Facility facility, Device device, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                switch (device.DeviceTypeId)
                {
                    //if staringDateTime != null senin başlangıç indexi staringDateTime.MinuteIndex;
                    case (int)DeviceTypeEnum.SolarPanel:
                        await StreamSolarDataAsync(facility.GrpcChannel, device, cancellationToken);
                        break;
                    case (int)DeviceTypeEnum.Bess:
                        await StreamBESSDataAsync(facility.GrpcChannel, device, cancellationToken);
                        break;
                    case (int)DeviceTypeEnum.PCS:
                        await StreamPCSDataAsync(facility.GrpcChannel, device, cancellationToken);
                        break;
                    case (int)DeviceTypeEnum.SmartMeter:
                        await StreamSmartMeterDataAsync(facility.GrpcChannel, device, cancellationToken);
                        break;
                    case (int)DeviceTypeEnum.FacilitySmartMeter:
                        await StreamFacilitySmartMeterDataAsync(facility.GrpcChannel, device, cancellationToken);
                        break;
                    case (int)DeviceTypeEnum.Generator:
                        await StreamSystemDataAsync(facility.GrpcChannel, device, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{device.DeviceType} {device.Id}] Error: {ex.Message}");
                _logger.LogError($"[Facility {device.DeviceType}] Hata Verdi : {ex.Message}. : {ex.StackTrace}");
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken); // 30 saniye sonra tekrar dene
            }
        }
    }


    public void StopFacilityStreams(int facilityId)
    {
        if (_cancellationTokens.TryRemove(facilityId, out var tokenSources))
        {
            foreach (var cts in tokenSources.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
        _facilityDeviceTasks.TryRemove(facilityId, out _);
    }



    private async Task StreamSolarDataAsync(string facilitychannel, Device device, CancellationToken cancellationToken)
    {
        var channel = GrpcChannel.ForAddress(facilitychannel);
        var client = new Ems.Solar.Stream.PPCService.PPCServiceClient(channel);
        var request = new SolarDataRequest { Id = (uint)device.Id, PeriodS = 60 };

        using var call = client.StreamSolarData(request);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var data = call.ResponseStream.Current;
            var entity = _mapper.Map<AppSolarData>(data);
            var solarDto = _mapper.Map<SolarDeviceDto>(data);
            entity.SolarId = device.Id;
            entity.TenantId = device.TenantId;
            entity.FacilityId = device.FacilityId;

            solarDto.DeviceId = device.Id;
            solarDto.NodeId = device.TopologyNode?.Id;
            solarDto.DeviceName=device.DeviceName;
            solarDto.Weather= _externalDataService.GetValueByDateTime("external_Weather_Temperature_C",DateTime.Now);
            solarDto.Irradiance= _externalDataService.GetValueByDateTime("external_Irradiance_wm2", DateTime.Now);

            var hubDto = _faciltyHubEntites.GetOrAdd(device.FacilityId, new FacilityHubDto());
            lock (hubDto.SolarDevices)
            {
                hubDto.SolarDevices.RemoveAll(x => x.DeviceId == device.Id);
                hubDto.SolarDevices.Add(solarDto);
            }

            await ScheduleDebouncedUpdate(device.FacilityId);
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.SolarDatas.AddAsync(entity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"{device.DeviceName}:adlı solar cihazı içi veri ekleme başarılı");

            }
        }
    }
    private async Task StreamBESSDataAsync(string facilitychannel, Device device, CancellationToken cancellationToken)
    {
        var channel = GrpcChannel.ForAddress(facilitychannel);
        var client = new Ems.Bess.Stream.BESSService.BESSServiceClient(channel);
        var request = new BESSDataRequest { Id = (uint)device.Id, PeriodS = 60 };

        using var call = client.StreamBESSData(request);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var data = call.ResponseStream.Current;
            var entity = _mapper.Map<AppBESSData>(data);
            entity.BESSId = device.Id;
            entity.TenantId = device.TenantId;
            entity.FacilityId = device.FacilityId;

            var deviceDto = _mapper.Map<BESSDeviceDto>(data);
            deviceDto.DeviceId = device.Id;
            deviceDto.NodeId = device.TopologyNode?.Id;
            deviceDto.DeviceName = device.DeviceName;

            var hubDto = _faciltyHubEntites.GetOrAdd(device.FacilityId, new FacilityHubDto());
            lock (hubDto.BessDevices)
            {
                hubDto.BessDevices.RemoveAll(x => x.DeviceId == device.Id);
                hubDto.BessDevices.Add(deviceDto);
            }

            await ScheduleDebouncedUpdate(device.FacilityId);

            using (var scope = _scopeFactory.CreateScope())
            {

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // entiy.created = datatime.now.today.addmilisiseconds (timer.totalmiliseconds);
                await context.BESSDatas.AddAsync(entity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"{device.DeviceName}:adlı BESS cihazı içi veri ekleme başarılı");

            }

        }
    }
    private async Task StreamPCSDataAsync(string facilitychannel, Device device, CancellationToken cancellationToken)
    {
        var channel = GrpcChannel.ForAddress(facilitychannel);
        var client = new Ems.Pcs.Stream.PCService.PCServiceClient(channel);
        var request = new PCSDataRequest { Id = (uint)device.Id, PeriodS = 60 };

        using var call = client.StreamPCSData(request);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var data = call.ResponseStream.Current;
            var entity = _mapper.Map<AppPCSData>(data);
            entity.PCSId = device.Id;
            entity.TenantId = device.TenantId;
            entity.FacilityId = device.FacilityId;


            var deviceDto = _mapper.Map<PCSDeviceDto>(data);
            deviceDto.DeviceId = device.Id;
            deviceDto.NodeId = device.TopologyNode?.Id;
            deviceDto.DeviceName = device.DeviceName;

            var hubDto = _faciltyHubEntites.GetOrAdd(device.FacilityId, new FacilityHubDto());
            lock (hubDto.PcsDevices)
            {
                hubDto.PcsDevices.RemoveAll(x => x.DeviceId == device.Id);
                hubDto.PcsDevices.Add(deviceDto);
            }

            await ScheduleDebouncedUpdate(device.FacilityId);

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.PCSDatas.AddAsync(entity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"{device.DeviceName}:adlı PCS cihazı içi veri ekleme başarılı");

            }

        }
    }
    private async Task StreamFacilitySmartMeterDataAsync(string facilitychannel, Device device, CancellationToken cancellationToken)
    {
        var channel = GrpcChannel.ForAddress(facilitychannel);
        var client = new Ems.Smartmeter.Stream.SmartMeterService.SmartMeterServiceClient(channel);
        var request = new SmartMeterDataRequest { Id = (uint)device.Id, PeriodS = 60 };

        using var call = client.StreamSmartMeterData(request);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var data = call.ResponseStream.Current;
            var entity = _mapper.Map<AppSmartMeterData>(data);
            entity.SmartMeterId = device.Id;
            entity.TenantId = device.TenantId;
            entity.FacilityId = device.FacilityId;

            var hubDto = _faciltyHubEntites.GetOrAdd(device.FacilityId, new FacilityHubDto());
            var deviceDto = _mapper.Map<FacilityPowerDto>(data);
            deviceDto.DeviceId = device.Id;
            deviceDto.NodeId = device.TopologyNode?.Id;
            hubDto.FacilityData = deviceDto;

            await ScheduleDebouncedUpdate(device.FacilityId);

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.SmartMeterDatas.AddAsync(entity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"{device.DeviceName}:adlı Smart cihazı içi veri ekleme başarılı");
            }
        }
    }
    private async Task StreamSmartMeterDataAsync(string facilitychannel, Device device, CancellationToken cancellationToken)
    {
        var channel = GrpcChannel.ForAddress(facilitychannel);
        var client = new Ems.Smartmeter.Stream.SmartMeterService.SmartMeterServiceClient(channel);
        var request = new SmartMeterDataRequest { Id = (uint)device.Id, PeriodS = 60 };

        using var call = client.StreamSmartMeterData(request);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var data = call.ResponseStream.Current;
            var entity = _mapper.Map<AppSmartMeterData>(data);
            entity.SmartMeterId = device.Id;
            entity.TenantId = device.TenantId;
            entity.FacilityId = device.FacilityId;


            var hubDto = _faciltyHubEntites.GetOrAdd(device.FacilityId, new FacilityHubDto());
            if (device.ConnectedDevice != null)
            {
                if (device.ConnectedDevice.DeviceTypeId == (int)DeviceTypeEnum.Grid)
                {
                    var deviceDto = _mapper.Map<GridPowerDto>(data);
                    deviceDto.DeviceName = device.ConnectedDevice.DeviceName;
                    deviceDto.DeviceId = device.ConnectedDevice.Id;
                    try
                    {
                        var nodeID = device.ConnectedDevice.TopologyNode?.Id;
                        deviceDto.NodeId = nodeID;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                    lock (hubDto.GridDevices)
                    {
                        hubDto.GridDevices.RemoveAll(x => x.DeviceId == device.ConnectedDevice.Id);
                        hubDto.GridDevices.Add(deviceDto);
                    }
                }
                else if (device.ConnectedDevice.DeviceTypeId == (uint)DeviceTypeEnum.Heat_Pump)
                {
                    //şuanverielde olmadığı için yapmıyoruz
                }
                else if (device.ConnectedDevice.DeviceTypeId == (uint)DeviceTypeEnum.EV_Chanrger)
                {
                    //şuanverielde olmadığı için yapmıyoruz
                }

            }
            await ScheduleDebouncedUpdate(device.FacilityId);

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.SmartMeterDatas.AddAsync(entity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"{device.DeviceName}:adlı Smart cihazı içi veri ekleme başarılı");

            }

        }
    }
    private async Task StreamSystemDataAsync(string facilitychannel, Device device, CancellationToken cancellationToken)
    {
        //TODO:şuan veride systemden geliyor sonrasında kendi protosondan gelecek
        var channel = GrpcChannel.ForAddress(facilitychannel);
        var client = new Ems.System.Stream.SystemService.SystemServiceClient(channel);
        var request = new Ems.System.Stream.SystemDataRequest { Id = (uint)device.FacilityId, PeriodS = 60 };

        using var call = client.StreamSystemData(request);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var data = call.ResponseStream.Current;
            var entity = _mapper.Map<AppGrpcSystemData>(data);
            entity.TenantId = device.TenantId;
            entity.FacilityId = device.FacilityId;

            var hubDto = _faciltyHubEntites.GetOrAdd(device.FacilityId, new FacilityHubDto());
            var systemDto = _mapper.Map<SystemDataDto>(data);

            var generatorDto = new GeneratorDeviceDto
            {
                DeviceId = device.Id,
                LoadGeneratorContribution = systemDto.LoadGeneratorContribution,
                TotalPower = systemDto.GeneratorPower,
                NodeId = device.TopologyNode?.Id,
            };

            systemDto.Cost_Saving_TL_Daily = _externalDataService.GetValueByDateTime("external_Cost_Saving_TL_Daily", DateTime.Now);
            systemDto.Grid_Consumption_TL_Daily = _externalDataService.GetValueByDateTime("external_Grid_Consumption_TL_Daily", DateTime.Now);
            hubDto.SystemData = systemDto;
            lock (hubDto.GeneratorDevices)
            {
                hubDto.GeneratorDevices.RemoveAll(x => x.DeviceId == device.Id);
                hubDto.GeneratorDevices.Add(generatorDto);
            }

            await ScheduleDebouncedUpdate(device.FacilityId);


            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.GrpcFacilityDatas.AddAsync(entity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                Console.WriteLine($"facişity verisi eklendi");
            }
        }
    }
}
