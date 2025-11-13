using Backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _dbContext;
    private IDbContextTransaction _transaction;

    // _transaction nullable olmayan bir alan olarak kabul edilecek, bu yüzden 'required' keywordü eklenebilir
    public UnitOfWork(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _transaction = null!;
    }

    // DbContext'e erişim sağlayan özellik
    public IApplicationDbContext DbContext => _dbContext;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
        return _transaction!;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
    }

    // IApplicationDbContext implementasyonu olan DbContext'te Dispose metodunu çağırıyoruz
    public void Dispose()
    {
        if (_dbContext is DbContext context)
        {
            context.Dispose();
        }
        _transaction?.Dispose();
    }
}
