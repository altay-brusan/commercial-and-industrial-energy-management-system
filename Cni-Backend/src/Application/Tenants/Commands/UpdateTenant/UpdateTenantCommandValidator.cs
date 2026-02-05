namespace Backend.Application.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(500)
            .NotEmpty();
        RuleFor(v => v.Identifier)
            .MaximumLength(250);
    }
}
