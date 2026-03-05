namespace Backend.Application.MaintenanceTasks.Commands.CompleteMaintenanceTask;

public class CompleteMaintenanceTaskCommandValidator : AbstractValidator<CompleteMaintenanceTaskCommand>
{
    public CompleteMaintenanceTaskCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Resolution)
            .NotEmpty().WithMessage("Resolution is required.")
            .MaximumLength(2000);

        RuleFor(v => v.RootCause)
            .MaximumLength(2000).When(v => v.RootCause != null);

        RuleFor(v => v.PartsUsed)
            .MaximumLength(1000).When(v => v.PartsUsed != null);

        RuleFor(v => v.Recommendations)
            .MaximumLength(2000).When(v => v.Recommendations != null);
    }
}
