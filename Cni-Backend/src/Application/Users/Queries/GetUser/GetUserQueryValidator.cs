namespace Backend.Application.Users.Queries.GetUser;

public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
