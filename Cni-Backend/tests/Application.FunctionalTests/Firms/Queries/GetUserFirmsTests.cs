using Backend.Application.Firms.Queries.GetUserFirms;
using Backend.Application.Firms.Queries.GetUserFirmFacilities;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Firms.Queries;

public class GetUserFirmsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnEmptyWhenNoFirmsAssigned()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetUserFirmsQuery());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldReturnAssignedFirms()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Test Tenant", Identifier = "test-tenant-firms" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Test Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Marmara",
            City = "Istanbul",
            Address = "Test Address 1",
            ContactEmail = "test@test.com",
            ContactPhone = "+90000000000",
            TaxNumber = "TR000000001",
            TaxOffice = "Istanbul"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Test Facility",
            EnergyCapacity = 100,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var uff = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm.Id,
            FacilityId = facility.Id,
            TenantId = tenant.Id
        };
        await Testing.AddAsync(uff);

        // Act
        var result = await Testing.SendAsync(new GetUserFirmsQuery());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(firm.Id);
        result.First().Name.Should().Be("Test Firm");
    }

    [Test]
    public async Task ShouldReturnDistinctFirms()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Distinct Tenant", Identifier = "distinct-tenant-firms" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Single Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Central Anatolia",
            City = "Ankara",
            Address = "Test Address 2",
            ContactEmail = "test2@test.com",
            ContactPhone = "+90000000001",
            TaxNumber = "TR000000002",
            TaxOffice = "Ankara"
        };
        await Testing.AddAsync(firm);

        var facility1 = new Facility
        {
            FirmId = firm.Id,
            Name = "Facility A",
            EnergyCapacity = 200,
            EnergyUnit = EnergyUnit.KWh
        };
        var facility2 = new Facility
        {
            FirmId = firm.Id,
            Name = "Facility B",
            EnergyCapacity = 300,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility1);
        await Testing.AddAsync(facility2);

        var uff1 = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm.Id,
            FacilityId = facility1.Id,
            TenantId = tenant.Id
        };
        var uff2 = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm.Id,
            FacilityId = facility2.Id,
            TenantId = tenant.Id
        };
        await Testing.AddAsync(uff1);
        await Testing.AddAsync(uff2);

        // Act
        var result = await Testing.SendAsync(new GetUserFirmsQuery());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Single Firm");
    }

    [Test]
    public async Task ShouldReturnEmptyWhenNoFacilitiesAssigned()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Empty Facilities Tenant", Identifier = "empty-facilities-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Empty Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Aegean",
            City = "Izmir",
            Address = "Test Address 3",
            ContactEmail = "test3@test.com",
            ContactPhone = "+90000000002",
            TaxNumber = "TR000000003",
            TaxOffice = "Izmir"
        };
        await Testing.AddAsync(firm);

        // Act
        var result = await Testing.SendAsync(new GetUserFirmFacilitiesQuery { FirmId = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldReturnAssignedFacilities()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Facilities Tenant", Identifier = "facilities-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Facilities Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Marmara",
            City = "Istanbul",
            Address = "Test Address 4",
            ContactEmail = "test4@test.com",
            ContactPhone = "+90000000003",
            TaxNumber = "TR000000004",
            TaxOffice = "Istanbul"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Solar Park Alpha",
            Address = "Test Address 123",
            EnergyCapacity = 500,
            EnergyUnit = EnergyUnit.KWh,
            Longitude = 28.9784,
            Latitude = 41.0082
        };
        await Testing.AddAsync(facility);

        var uff = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm.Id,
            FacilityId = facility.Id,
            TenantId = tenant.Id
        };
        await Testing.AddAsync(uff);

        // Act
        var result = await Testing.SendAsync(new GetUserFirmFacilitiesQuery { FirmId = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Solar Park Alpha");
        result.First().FirmId.Should().Be(firm.Id);
        result.First().EnergyCapacity.Should().Be(500);
    }

    [Test]
    public async Task ShouldOnlyReturnFacilitiesForSpecifiedFirm()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Multi Firm Tenant", Identifier = "multi-firm-tenant" };
        await Testing.AddAsync(tenant);

        var firm1 = new Firm
        {
            TenantId = tenant.Id,
            Name = "Firm Alpha",
            Type = "Standard",
            Country = "Turkey",
            State = "Marmara",
            City = "Istanbul",
            Address = "Test Address 5",
            ContactEmail = "alpha@test.com",
            ContactPhone = "+90000000004",
            TaxNumber = "TR000000005",
            TaxOffice = "Istanbul"
        };
        var firm2 = new Firm
        {
            TenantId = tenant.Id,
            Name = "Firm Beta",
            Type = "Standard",
            Country = "Turkey",
            State = "Central Anatolia",
            City = "Ankara",
            Address = "Test Address 6",
            ContactEmail = "beta@test.com",
            ContactPhone = "+90000000005",
            TaxNumber = "TR000000006",
            TaxOffice = "Ankara"
        };
        await Testing.AddAsync(firm1);
        await Testing.AddAsync(firm2);

        var facility1 = new Facility
        {
            FirmId = firm1.Id,
            Name = "Alpha Facility",
            EnergyCapacity = 100,
            EnergyUnit = EnergyUnit.KWh
        };
        var facility2 = new Facility
        {
            FirmId = firm2.Id,
            Name = "Beta Facility",
            EnergyCapacity = 200,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility1);
        await Testing.AddAsync(facility2);

        var uff1 = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm1.Id,
            FacilityId = facility1.Id,
            TenantId = tenant.Id
        };
        var uff2 = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm2.Id,
            FacilityId = facility2.Id,
            TenantId = tenant.Id
        };
        await Testing.AddAsync(uff1);
        await Testing.AddAsync(uff2);

        // Act
        var result = await Testing.SendAsync(new GetUserFirmFacilitiesQuery { FirmId = firm1.Id });

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Alpha Facility");
    }

    [Test]
    public async Task ShouldReturnDistinctFacilities()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var tenant = new Tenant { Name = "Distinct Facilities Tenant", Identifier = "distinct-facilities-tenant" };
        await Testing.AddAsync(tenant);

        var firm = new Firm
        {
            TenantId = tenant.Id,
            Name = "Distinct Firm",
            Type = "Standard",
            Country = "Turkey",
            State = "Marmara",
            City = "Bursa",
            Address = "Test Address 7",
            ContactEmail = "distinct@test.com",
            ContactPhone = "+90000000006",
            TaxNumber = "TR000000007",
            TaxOffice = "Bursa"
        };
        await Testing.AddAsync(firm);

        var facility = new Facility
        {
            FirmId = firm.Id,
            Name = "Unique Facility",
            EnergyCapacity = 150,
            EnergyUnit = EnergyUnit.KWh
        };
        await Testing.AddAsync(facility);

        var uff = new UserFirmFacility
        {
            UserId = userId,
            FirmId = firm.Id,
            FacilityId = facility.Id,
            TenantId = tenant.Id
        };
        await Testing.AddAsync(uff);

        // Act
        var result = await Testing.SendAsync(new GetUserFirmFacilitiesQuery { FirmId = firm.Id });

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Unique Facility");
    }
}
