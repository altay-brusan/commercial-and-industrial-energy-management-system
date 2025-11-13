using Backend.Domain.Entities;
using Backend.Domain.Entities.Proto;
using Backend.Domain.Entities.Topology;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Firm> Firms { get; }
    DbSet<Facility> Facilities { get; }
    DbSet<Device> Devices { get; }
    DbSet<DeviceType> DeviceTypes { get; }
    DbSet<UserFirmFacility> UserFirmFacilities { get; }
    DbSet<TopologyNode> TopologyNodes { get; }
    DbSet<TopologyEdge> TopologyEdges { get; }
    DbSet<AppSolarData> SolarDatas { get; }
    DbSet<AppBESSData> BESSDatas { get; }
    DbSet<AppPCSData> PCSDatas { get; }
    DbSet<AppSmartMeterData> SmartMeterDatas { get; }
    DbSet<AppGrpcSystemData> GrpcFacilityDatas { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    // Transaction desteği için yeni metot
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
