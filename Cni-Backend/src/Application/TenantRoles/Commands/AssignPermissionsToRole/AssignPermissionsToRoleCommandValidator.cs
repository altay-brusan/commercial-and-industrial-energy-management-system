namespace Backend.Application.TenantRoles.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleCommandValidator()
    {
        RuleFor(v => v.TenantRoleId)
            .NotEmpty();

        RuleFor(v => v.PermissionIds)
            .NotNull();
    }
}
