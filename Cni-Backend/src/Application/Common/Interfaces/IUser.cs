namespace Backend.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    string? TenantId { get; }  // Yeni ekleme
}
