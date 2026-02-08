namespace Backend.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RequirePermissionAttribute : Attribute
{
    public RequirePermissionAttribute(string permission) => Permission = permission;
    public string Permission { get; }
    public string? Feature { get; set; }
}
