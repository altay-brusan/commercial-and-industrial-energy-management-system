namespace Backend.Application.AppRoles.Commands.AddRoleClaim;

public class AddRoleClaimCommandValidator : AbstractValidator<AddRoleClaimCommand>
{
    public AddRoleClaimCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().WithMessage("Role adı boş olamaz.");
        RuleFor(x => x.ClaimValue).NotEmpty().WithMessage("Claim değeri boş olamaz.");
    }
}
