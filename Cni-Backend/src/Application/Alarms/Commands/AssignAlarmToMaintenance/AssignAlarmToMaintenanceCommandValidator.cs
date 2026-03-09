namespace Backend.Application.Alarms.Commands.AssignAlarmToMaintenance;

public class AssignAlarmToMaintenanceCommandValidator : AbstractValidator<AssignAlarmToMaintenanceCommand>
{
    public AssignAlarmToMaintenanceCommandValidator()
    {
        RuleFor(x => x.AlarmId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => new[] { "critical", "high", "medium", "low" }
                .Contains(p.ToLower()))
            .WithMessage("Priority must be Critical, High, Medium, or Low.");

        RuleFor(x => x.AssignedToId)
            .NotEmpty();

        RuleFor(x => x.DueDate)
            .NotEmpty();
    }
}
