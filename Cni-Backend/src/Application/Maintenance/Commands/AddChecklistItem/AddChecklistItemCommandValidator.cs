namespace Backend.Application.Maintenance.Commands.AddChecklistItem;

public class AddChecklistItemCommandValidator : AbstractValidator<AddChecklistItemCommand>
{
    public AddChecklistItemCommandValidator()
    {
        RuleFor(v => v.TaskId).NotEmpty();
        RuleFor(v => v.Label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(500);
    }
}
