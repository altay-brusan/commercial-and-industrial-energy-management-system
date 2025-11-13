namespace Backend.Domain.Entities;

public class Tenant : BaseAuditableEntity
{
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public string? ConnectionString { get; set; } // Aynı veritabanını kullandığınız için boş kalabilir
    public string? Items { get; set; } // Ek bilgiler için kullanılabilir.
    public ICollection<Firm>? Firms { get; set; }
}
