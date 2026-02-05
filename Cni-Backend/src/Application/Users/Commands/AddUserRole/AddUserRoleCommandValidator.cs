namespace Backend.Application.Users.Commands.AddUserRole;

public class AddUserRoleCommandValidator : AbstractValidator<AddUserRoleCommand>
{
    public AddUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty();
    }
}
