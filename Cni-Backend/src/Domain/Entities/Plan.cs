namespace Backend.Domain.Entities;
public class Plan : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Modules { get; set; } = string.Empty; // JSON veya CSV formatında tutulabilir
    public bool ApiAccess { get; set; }
    public int MaxFacilities { get; set; }
    public int MaxUsers { get; set; }
    public decimal Price { get; set; }
}
