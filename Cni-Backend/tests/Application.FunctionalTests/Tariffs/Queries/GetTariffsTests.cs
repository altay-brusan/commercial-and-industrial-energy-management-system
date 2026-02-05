using Backend.Application.Tariffs.Queries.GetTariffs;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Tariffs.Queries;

public class GetTariffsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnPaginatedTariffs()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        for (int i = 1; i <= 15; i++)
        {
            var tariff = new Tariff
            {
                CountryName = $"Country{i:D2}",
                DistributionFee = 20m + i,
                TransmissionFee = 5m + i,
                TaxRate = 0.20m,
                Currency = "EUR",
                IsActive = true
            };
            await Testing.AddAsync(tariff);
        }

        // Act
        var result = await Testing.SendAsync(new GetTariffsQuery { PageNumber = 1, PageSize = 10 });

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);
    }

    [Test]
    public async Task ShouldReturnSecondPage()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        for (int i = 1; i <= 15; i++)
        {
            var tariff = new Tariff
            {
                CountryName = $"Country{i:D2}",
                DistributionFee = 20m + i,
                TransmissionFee = 5m + i,
                TaxRate = 0.20m,
                Currency = "EUR",
                IsActive = true
            };
            await Testing.AddAsync(tariff);
        }

        // Act
        var result = await Testing.SendAsync(new GetTariffsQuery { PageNumber = 2, PageSize = 10 });

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
    }

    [Test]
    public async Task ShouldFilterByActiveStatus()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var activeTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(activeTariff);

        var inactiveTariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(inactiveTariff);

        // Act - Filter by active only
        var activeResult = await Testing.SendAsync(new GetTariffsQuery { IsActive = true });

        // Assert
        activeResult.Items.Should().HaveCount(1);
        activeResult.Items.First().CountryName.Should().Be("Netherlands");
    }

    [Test]
    public async Task ShouldFilterByInactiveStatus()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var activeTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(activeTariff);

        var inactiveTariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(inactiveTariff);

        // Act - Filter by inactive only
        var inactiveResult = await Testing.SendAsync(new GetTariffsQuery { IsActive = false });

        // Assert
        inactiveResult.Items.Should().HaveCount(1);
        inactiveResult.Items.First().CountryName.Should().Be("Germany");
    }

    [Test]
    public async Task ShouldFilterByCountryName()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff1 = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff1);

        var tariff2 = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff2);

        var tariff3 = new Tariff
        {
            CountryName = "Belgium",
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff3);

        // Act - Filter by country name
        var result = await Testing.SendAsync(new GetTariffsQuery { CountryName = "ger" });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CountryName.Should().Be("Germany");
    }

    [Test]
    public async Task ShouldReturnAllWhenNoFilters()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var activeTariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(activeTariff);

        var inactiveTariff = new Tariff
        {
            CountryName = "Germany",
            DistributionFee = 30m,
            TransmissionFee = 8m,
            TaxRate = 0.19m,
            Currency = "EUR",
            IsActive = false
        };
        await Testing.AddAsync(inactiveTariff);

        // Act - No filters
        var result = await Testing.SendAsync(new GetTariffsQuery());

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Test]
    public async Task ShouldReturnEmptyWhenNoMatchingTariffs()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff = new Tariff
        {
            CountryName = "Netherlands",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff);

        // Act
        var result = await Testing.SendAsync(new GetTariffsQuery { CountryName = "France" });

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Test]
    public async Task ShouldOrderByCountryNameThenByCreatedDescending()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        var tariff1 = new Tariff
        {
            CountryName = "Belgium",
            DistributionFee = 22m,
            TransmissionFee = 6m,
            TaxRate = 0.21m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff1);

        var tariff2 = new Tariff
        {
            CountryName = "Austria",
            DistributionFee = 20m,
            TransmissionFee = 5m,
            TaxRate = 0.20m,
            Currency = "EUR",
            IsActive = true
        };
        await Testing.AddAsync(tariff2);

        // Act
        var result = await Testing.SendAsync(new GetTariffsQuery());

        // Assert
        result.Items.First().CountryName.Should().Be("Austria");
    }
}
