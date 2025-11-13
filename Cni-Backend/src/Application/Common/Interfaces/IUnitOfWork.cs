using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Application.Common.Interfaces;
public interface IUnitOfWork : IDisposable
{
    IApplicationDbContext DbContext { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
