using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Security;
using Backend.Domain.Entities;

namespace Backend.Application.Tariffs.Commands.CreateTariff;

/// <summary>
/// Creates a new tariff configuration.
/// If a tariff already exists for the country, the existing active tariff(s) will be deactivated.
/// Only Administrators can create tariffs.
/// </summary>
[Authorize(Roles = "Administrator")]
public record CreateTariffCommand : IRequest<Guid>
{
    public string CountryName { get; init; } = string.Empty;
    public string? DistributorName { get; init; }
    public decimal DistributionFee { get; init; }
    public decimal TransmissionFee { get; init; }
    public decimal TaxRate { get; init; }
    public string Currency { get; init; } = "EUR";
}

public class CreateTariffCommandHandler : IRequestHandler<CreateTariffCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTariffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
    {
        // Use a transaction to ensure atomicity
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Deactivate any existing active tariffs for this country
            var existingActiveTariffs = await _context.Tariffs
                .Where(t => t.IsActive && t.CountryName.ToLower() == request.CountryName.ToLower())
                .ToListAsync(cancellationToken);

            foreach (var existingTariff in existingActiveTariffs)
            {
                existingTariff.IsActive = false;
            }

            // Create the new tariff
            var tariff = new Tariff
            {
                CountryName = request.CountryName,
                DistributorName = request.DistributorName,
                DistributionFee = request.DistributionFee,
                TransmissionFee = request.TransmissionFee,
                TaxRate = request.TaxRate,
                Currency = request.Currency,
                IsActive = true
            };

            _context.Tariffs.Add(tariff);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return tariff.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
