namespace Backend.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(500);
    }
}
