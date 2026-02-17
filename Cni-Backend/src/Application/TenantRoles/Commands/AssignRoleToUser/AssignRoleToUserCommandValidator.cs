namespace Backend.Application.TenantRoles.Commands.AssignRoleToUser;

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(v => v.TenantRoleId)
            .GreaterThan(0);

        RuleFor(v => v.UserId)
            .NotEmpty();
    }
}
