namespace Backend.Application.LogicalNodes.Commands;

public class CreateLogicalNodeCommandValidator : AbstractValidator<CreateLogicalNodeCommand>
{
    public CreateLogicalNodeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DisplayName)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Parameters)
            .NotEmpty();
    }
}
