namespace Backend.Application.TenantRoles.Commands.CreateTenantRole;

public class CreateTenantRoleCommandValidator : AbstractValidator<CreateTenantRoleCommand>
{
    public CreateTenantRoleCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Description)
            .MaximumLength(500);
    }
}
