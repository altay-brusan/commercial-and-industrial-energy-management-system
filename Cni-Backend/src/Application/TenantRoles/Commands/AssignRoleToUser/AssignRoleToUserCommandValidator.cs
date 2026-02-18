namespace Backend.Application.TenantRoles.Commands.AssignRoleToUser;

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(v => v.TenantRoleId)
            .NotEmpty();

        RuleFor(v => v.UserId)
            .NotEmpty();
    }
}
