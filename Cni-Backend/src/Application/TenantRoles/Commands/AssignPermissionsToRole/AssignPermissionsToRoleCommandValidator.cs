namespace Backend.Application.TenantRoles.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleCommandValidator()
    {
        RuleFor(v => v.TenantRoleId)
            .GreaterThan(0);

        RuleFor(v => v.PermissionIds)
            .NotNull();
    }
}
