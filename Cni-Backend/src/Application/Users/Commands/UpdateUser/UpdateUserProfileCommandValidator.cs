namespace Backend.Application.Users.Commands.UpdateUser;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(v => v.Name)
             .NotEmpty()
             .MaximumLength(100);
        RuleFor(v => v.Email)
           .NotEmpty()
           .When(v => !string.IsNullOrEmpty(v.Email))
           .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
    }
}
