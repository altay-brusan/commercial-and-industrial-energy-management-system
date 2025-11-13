namespace Backend.Domain.Common;
public interface IMultiTenantEntity
{
    public int TenantId { get; set; }
}
