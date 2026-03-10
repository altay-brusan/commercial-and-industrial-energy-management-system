using Backend.Application.Alarms.Commands.AcknowledgeAlarm;
using Backend.Application.Alarms.Commands.AssignAlarmToMaintenance;
using Backend.Application.Alarms.Commands.ResolveAlarm;
using Backend.Application.Alarms.Queries.GetAlarm;
using Backend.Application.Alarms.Queries.GetAlarmHistory;
using Backend.Application.Alarms.Queries.GetAlarms;
using Backend.Application.Alarms.Queries.GetAlarmStatistics;
using Backend.Application.Alarms.Queries.GetAlarmSummary;
using Backend.Application.Common.Exceptions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Alarms;

public class AlarmTests : BaseTestFixture
{
    private Tenant _tenant = null!;
    private Firm _firm = null!;
    private Facility _facility = null!;
    private DeviceType _deviceType = null!;
    private Rtu _rtu = null!;
    private Device _device = null!;
    private DeviceAlarmType _alarmType = null!;
    private int _alarmTypeCounter;

    private async Task SetupBaseEntitiesAsync()
    {
        _tenant = new Tenant { Name = "Test Tenant" };
        await Testing.AddAsync(_tenant);

        _firm = new Firm
        {
            TenantId = _tenant.Id,
            Name = "Test Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Istanbul",
            City = "Istanbul",
            Address = "Test Address 1",
            ContactEmail = "test@firm.com",
            ContactPhone = "+905551234567",
            TaxNumber = "1234567890",
            TaxOffice = "Istanbul"
        };
        await Testing.AddAsync(_firm);

        _facility = new Facility
        {
            FirmId = _firm.Id,
            Name = "Test Facility",
            EnergyCapacity = 100,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(_facility);

        _deviceType = new DeviceType { Name = "EnergyStorage" };
        await Testing.AddAsync(_deviceType);

        _rtu = new Rtu
        {
            FacilityId = _facility.Id,
            RtuSerialNumber = "RTU-TEST-001",
            Protocol = "Modbus"
        };
        await Testing.AddAsync(_rtu);

        _device = new Device
        {
            RtuId = _rtu.Id,
            DeviceTypeId = _deviceType.Id,
            Name = "BESS_TEST",
            SerialNumber = "BESS-SN-TEST"
        };
        await Testing.AddAsync(_device);

        _alarmType = new DeviceAlarmType
        {
            DeviceTypeId = _deviceType.Id,
            Code = "TEST_ALARM",
            Name = "Test Alarm",
            FaultKey = "testFault",
            Kind = "Battery.Status",
            Severity = "Critical",
            Category = "battery",
            Source = "BMS",
            Model = "TEST-MODEL",
            Manufacturer = "Test"
        };
        await Testing.AddAsync(_alarmType);
    }

    private async Task<DeviceAlarm> CreateActiveAlarmAsync(
        string? severity = null,
        bool isActive = true,
        bool isAcknowledged = false,
        string? deviceName = null)
    {
        _alarmTypeCounter++;
        var alarmTypeToUse = _alarmType;
        if (severity != null && severity != "Critical")
        {
            alarmTypeToUse = new DeviceAlarmType
            {
                DeviceTypeId = _deviceType.Id,
                Code = $"TEST_{severity.ToUpper()}_{_alarmTypeCounter}",
                Name = $"Test {severity} Alarm {_alarmTypeCounter}",
                FaultKey = $"test{severity}Fault{_alarmTypeCounter}",
                Kind = "Battery.Status",
                Severity = severity,
                Category = "battery",
                Source = "BMS",
                Model = "TEST-MODEL",
                Manufacturer = "Test"
            };
            await Testing.AddAsync(alarmTypeToUse);
        }

        var alarm = new DeviceAlarm
        {
            DeviceId = _device.Id,
            DeviceAlarmTypeId = alarmTypeToUse.Id,
            OccurredAt = DateTimeOffset.UtcNow.AddHours(-1),
            IsActive = isActive,
            IsRead = false,
            IsProcessed = false,
            Details = "{\"message\":\"Test alarm details\"}",
            RtuSerialNumber = "RTU-TEST-001",
            DeviceName = deviceName ?? "BESS_TEST"
        };

        if (isAcknowledged)
        {
            alarm.AcknowledgedAt = DateTimeOffset.UtcNow.AddMinutes(-30);
            alarm.AcknowledgedById = Testing.GetUserId();
        }

        if (!isActive)
        {
            alarm.ResolvedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
            alarm.ResolvedById = Testing.GetUserId();
        }

        await Testing.AddAsync(alarm);
        return alarm;
    }

    #region GetAlarms

    [Test]
    public async Task GetAlarms_ShouldReturnEmptyListWhenNoAlarms()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var result = await Testing.SendAsync(new GetAlarmsQuery());

        result.Alarms.Should().BeEmpty();
        result.Pagination.TotalItems.Should().Be(0);
        result.Pagination.TotalPages.Should().Be(0);
    }

    [Test]
    public async Task GetAlarms_ShouldReturnAllAlarms()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync();
        await CreateActiveAlarmAsync(severity: "Warning");
        await CreateActiveAlarmAsync(severity: "Minor");

        var result = await Testing.SendAsync(new GetAlarmsQuery());

        result.Alarms.Should().HaveCount(3);
        result.Pagination.TotalItems.Should().Be(3);
    }

    [Test]
    public async Task GetAlarms_ShouldFilterByFacilityId()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync();

        // Create alarm in a different facility
        var otherFacility = new Facility
        {
            FirmId = _firm.Id,
            Name = "Other Facility",
            EnergyCapacity = 200,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(otherFacility);

        var otherRtu = new Rtu
        {
            FacilityId = otherFacility.Id,
            RtuSerialNumber = "RTU-OTHER-001",
            Protocol = "Modbus"
        };
        await Testing.AddAsync(otherRtu);

        var otherDevice = new Device
        {
            RtuId = otherRtu.Id,
            DeviceTypeId = _deviceType.Id,
            Name = "BESS_OTHER",
            SerialNumber = "BESS-SN-OTHER"
        };
        await Testing.AddAsync(otherDevice);

        var otherAlarm = new DeviceAlarm
        {
            DeviceId = otherDevice.Id,
            DeviceAlarmTypeId = _alarmType.Id,
            OccurredAt = DateTimeOffset.UtcNow.AddHours(-1),
            IsActive = true,
            IsRead = false,
            IsProcessed = false,
            RtuSerialNumber = "RTU-OTHER-001",
            DeviceName = "BESS_OTHER"
        };
        await Testing.AddAsync(otherAlarm);

        var result = await Testing.SendAsync(new GetAlarmsQuery { FacilityId = _facility.Id });

        result.Alarms.Should().HaveCount(1);
        result.Alarms[0].Id.Should().NotBe(otherAlarm.Id);
    }

    [Test]
    public async Task GetAlarms_ShouldFilterBySeverity()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync(); // Critical
        await CreateActiveAlarmAsync(severity: "Warning");

        var result = await Testing.SendAsync(new GetAlarmsQuery { Severity = "critical" });

        result.Alarms.Should().HaveCount(1);
        result.Alarms[0].Severity.Should().Be("critical");
    }

    [Test]
    public async Task GetAlarms_ShouldFilterByStatusActive()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync(); // active, not acknowledged
        await CreateActiveAlarmAsync(isActive: false); // resolved

        var result = await Testing.SendAsync(new GetAlarmsQuery { Status = "active" });

        result.Alarms.Should().HaveCount(1);
        result.Alarms[0].Status.Should().Be("active");
    }

    [Test]
    public async Task GetAlarms_ShouldPaginate()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        for (int i = 0; i < 5; i++)
        {
            await CreateActiveAlarmAsync(severity: i % 2 == 0 ? "Critical" : "Warning");
        }

        var page1 = await Testing.SendAsync(new GetAlarmsQuery { Page = 1, PageSize = 2 });
        var page2 = await Testing.SendAsync(new GetAlarmsQuery { Page = 2, PageSize = 2 });
        var page3 = await Testing.SendAsync(new GetAlarmsQuery { Page = 3, PageSize = 2 });

        page1.Alarms.Should().HaveCount(2);
        page2.Alarms.Should().HaveCount(2);
        page3.Alarms.Should().HaveCount(1);
        page1.Pagination.TotalItems.Should().Be(5);
        page1.Pagination.TotalPages.Should().Be(3);
        page1.Pagination.Page.Should().Be(1);
        page1.Pagination.PageSize.Should().Be(2);
    }

    #endregion

    #region GetAlarm

    [Test]
    public async Task GetAlarm_ShouldReturnAlarmById()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync();

        var result = await Testing.SendAsync(new GetAlarmQuery { Id = alarm.Id });

        result.Should().NotBeNull();
        result.Id.Should().Be(alarm.Id);
        result.Code.Should().NotBeNullOrEmpty();
        result.Severity.Should().Be("critical");
        result.Status.Should().Be("active");
    }

    [Test]
    public async Task GetAlarm_ShouldThrowNotFoundForInvalidId()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new GetAlarmQuery { Id = Guid.NewGuid() }))
            .Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAlarmSummary

    [Test]
    public async Task GetAlarmSummary_ShouldReturnCorrectSummary()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync(); // Critical, active
        await CreateActiveAlarmAsync(); // Critical, active
        await CreateActiveAlarmAsync(severity: "Major"); // High, active
        await CreateActiveAlarmAsync(isAcknowledged: true); // Critical, acknowledged
        await CreateActiveAlarmAsync(isActive: false); // resolved (not counted in active)

        var result = await Testing.SendAsync(new GetAlarmSummaryQuery { FacilityId = _facility.Id });

        result.TotalActive.Should().Be(4); // all active (including acknowledged)
        result.Critical.Should().Be(3); // 2 active + 1 acknowledged critical
        result.High.Should().Be(1); // Major maps to High
        result.Acknowledged.Should().Be(1);
    }

    [Test]
    public async Task GetAlarmSummary_ShouldReturnZerosForNoAlarms()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var result = await Testing.SendAsync(new GetAlarmSummaryQuery { FacilityId = _facility.Id });

        result.TotalActive.Should().Be(0);
        result.Critical.Should().Be(0);
        result.High.Should().Be(0);
        result.Acknowledged.Should().Be(0);
    }

    #endregion

    #region GetAlarmStatistics

    [Test]
    public async Task GetAlarmStatistics_ShouldReturnCorrectStatistics()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync(); // Critical, active
        await CreateActiveAlarmAsync(severity: "Major"); // High/Major, active
        await CreateActiveAlarmAsync(severity: "Warning"); // Low/Warning, active
        await CreateActiveAlarmAsync(isAcknowledged: true); // Critical, acknowledged
        await CreateActiveAlarmAsync(isActive: false); // Critical, resolved

        var result = await Testing.SendAsync(new GetAlarmStatisticsQuery { FacilityId = _facility.Id });

        result.TotalAlarms.Should().Be(5);
        result.ActiveAlarms.Should().Be(3); // Critical active + Major active + Warning active (not acknowledged)
        result.CriticalAlarms.Should().Be(3); // 1 active + 1 acknowledged + 1 resolved
        result.HighAlarms.Should().Be(1);
        result.LowAlarms.Should().Be(1); // Warning maps to Low
        result.AcknowledgedCount.Should().Be(1);
        result.ResolvedCount.Should().Be(1);
        result.BySourceType.Should().ContainKey("battery");
    }

    #endregion

    #region GetAlarmHistory

    [Test]
    public async Task GetAlarmHistory_ShouldReturnHistoryForDevice()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync();
        await CreateActiveAlarmAsync(isActive: false);

        var result = await Testing.SendAsync(new GetAlarmHistoryQuery { DeviceName = "BESS_TEST" });

        result.Alarms.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAlarmHistory_ShouldReturnEmptyForUnknownDevice()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await CreateActiveAlarmAsync();

        var result = await Testing.SendAsync(new GetAlarmHistoryQuery { DeviceName = "UNKNOWN_DEVICE" });

        result.Alarms.Should().BeEmpty();
    }

    #endregion

    #region AcknowledgeAlarm

    [Test]
    public async Task AcknowledgeAlarm_ShouldAcknowledgeAlarm()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync();

        var result = await Testing.SendAsync(new AcknowledgeAlarmCommand { Id = alarm.Id });

        result.Should().NotBeNull();
        result.Id.Should().Be(alarm.Id);
        result.AcknowledgedAt.Should().NotBeNull();
        result.AcknowledgedBy.Should().NotBeNull();
        result.Status.Should().Be("acknowledged");
    }

    [Test]
    public async Task AcknowledgeAlarm_ShouldThrowNotFoundForInvalidAlarm()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new AcknowledgeAlarmCommand { Id = Guid.NewGuid() }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AcknowledgeAlarm_ShouldThrowWhenAlarmAlreadyAcknowledged()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync(isAcknowledged: true);

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new AcknowledgeAlarmCommand { Id = alarm.Id }))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task AcknowledgeAlarm_ShouldThrowWhenAlarmIsResolved()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync(isActive: false);

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new AcknowledgeAlarmCommand { Id = alarm.Id }))
            .Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region ResolveAlarm

    [Test]
    public async Task ResolveAlarm_ShouldResolveAlarm()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync();

        var result = await Testing.SendAsync(new ResolveAlarmCommand { Id = alarm.Id });

        result.Should().NotBeNull();
        result.Id.Should().Be(alarm.Id);
        result.ResolvedAt.Should().NotBeNull();
        result.ResolvedBy.Should().NotBeNull();
        result.Status.Should().Be("resolved");
    }

    [Test]
    public async Task ResolveAlarm_ShouldThrowNotFoundForInvalidAlarm()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new ResolveAlarmCommand { Id = Guid.NewGuid() }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ResolveAlarm_ShouldThrowWhenAlarmAlreadyResolved()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync(isActive: false);

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new ResolveAlarmCommand { Id = alarm.Id }))
            .Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region AssignAlarmToMaintenance

    [Test]
    public async Task AssignAlarmToMaintenance_ShouldCreateMaintenanceTaskAndLinkAlarm()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync();
        var userId = Testing.GetUserId()!;

        var result = await Testing.SendAsync(new AssignAlarmToMaintenanceCommand
        {
            AlarmId = alarm.Id,
            Title = "Fix critical battery alarm",
            Description = "Investigate and resolve the test alarm",
            Priority = "high",
            AssignedToId = userId,
            DueDate = DateTimeOffset.UtcNow.AddDays(7)
        });

        result.Should().NotBeNull();
        result.Task.Should().NotBeNull();
        result.Task.Title.Should().Be("Fix critical battery alarm");
        result.Task.Description.Should().Be("Investigate and resolve the test alarm");
        result.Task.Priority.Should().Be("high");
        result.Task.AssignedTo.Should().NotBeNull();
        result.Task.RelatedAlarm.Should().NotBeNull();
        result.Task.RelatedAlarm!.Id.Should().Be(alarm.Id);
        result.Alarm.Should().NotBeNull();
        result.Alarm.RelatedMaintenanceTaskId.Should().NotBeNull();
    }

    [Test]
    public async Task AssignAlarmToMaintenance_ShouldValidateRequiredFields()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var command = new AssignAlarmToMaintenanceCommand
        {
            AlarmId = Guid.Empty,
            Title = "",
            Priority = "",
            AssignedToId = ""
        };

        await FluentActions.Invoking(() =>
            Testing.SendAsync(command))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task AssignAlarmToMaintenance_ShouldThrowNotFoundForInvalidAlarm()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var userId = Testing.GetUserId()!;

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new AssignAlarmToMaintenanceCommand
            {
                AlarmId = Guid.NewGuid(),
                Title = "Fix alarm",
                Priority = "high",
                AssignedToId = userId,
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AssignAlarmToMaintenance_ShouldRejectInvalidPriority()
    {
        await Testing.RunAsAdministratorAsync();
        await SetupBaseEntitiesAsync();

        var alarm = await CreateActiveAlarmAsync();
        var userId = Testing.GetUserId()!;

        await FluentActions.Invoking(() =>
            Testing.SendAsync(new AssignAlarmToMaintenanceCommand
            {
                AlarmId = alarm.Id,
                Title = "Fix alarm",
                Priority = "invalid_priority",
                AssignedToId = userId,
                DueDate = DateTimeOffset.UtcNow.AddDays(7)
            }))
            .Should().ThrowAsync<ValidationException>();
    }

    #endregion
}
