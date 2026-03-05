namespace Backend.Application.MaintenanceTasks.Commands.AddTaskNote;

public class AddTaskNoteCommandValidator : AbstractValidator<AddTaskNoteCommand>
{
    public AddTaskNoteCommandValidator()
    {
        RuleFor(v => v.TaskId).NotEmpty();
        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(4000);
    }
}
