namespace Backend.Application.TenantRoles.Commands.UpdateTenantRole;

public class UpdateTenantRoleCommandValidator : AbstractValidator<UpdateTenantRoleCommand>
{
    public UpdateTenantRoleCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Description)
            .MaximumLength(500);
    }
}
