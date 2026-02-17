using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class RefreshToken : BaseAuditableEntity
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
