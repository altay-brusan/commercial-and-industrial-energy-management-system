namespace Backend.Domain.Common;
public interface IMultiTenantEntity
{
    public Guid TenantId { get; set; }
}
