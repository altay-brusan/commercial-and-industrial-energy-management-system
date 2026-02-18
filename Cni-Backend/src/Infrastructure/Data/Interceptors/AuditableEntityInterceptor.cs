using Backend.Application.Common.Interfaces;
using Backend.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;
    private readonly IMultiTenantService _multiTenantService;
    private readonly ILogger<AuditableEntityInterceptor> _logger;

    public AuditableEntityInterceptor(
        IUser user,
        TimeProvider dateTime,
        IMultiTenantService multiTenantService,
        ILogger<AuditableEntityInterceptor> logger)
    {
        _user = user;
        _dateTime = dateTime;
        _multiTenantService = multiTenantService;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var isImpersonating = _multiTenantService.IsImpersonating();
        var effectiveTenantId = _multiTenantService.GetEffectiveTenantId();

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _dateTime.GetUtcNow();
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.Created = utcNow;

                    // Log when admin creates entities while impersonating
                    if (isImpersonating)
                    {
                        LogImpersonationAction(entry, "created", effectiveTenantId);
                    }
                }
                else if (entry.State == EntityState.Modified && isImpersonating)
                {
                    // Log when admin modifies entities while impersonating
                    LogImpersonationAction(entry, "modified", effectiveTenantId);
                }

                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModified = utcNow;
            }
        }
    }

    private void LogImpersonationAction(EntityEntry entry, string action, Guid? tenantId)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityId = entry.Property("Id")?.CurrentValue;

        _logger.LogInformation(
            "Admin {UserId} {Action} {EntityType} (ID: {EntityId}) while impersonating tenant {TenantId}",
            _user.Id,
            action,
            entityType,
            entityId,
            tenantId);
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
