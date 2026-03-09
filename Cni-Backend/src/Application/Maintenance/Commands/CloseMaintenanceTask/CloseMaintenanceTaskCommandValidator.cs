using Backend.Domain.Constants;

namespace Backend.Application.Maintenance.Commands.CloseMaintenanceTask;

public class CloseMaintenanceTaskCommandValidator : AbstractValidator<CloseMaintenanceTaskCommand>
{
    public CloseMaintenanceTaskCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Reason)
            .NotEmpty().WithMessage("Close reason is required.")
            .Must(r => r is CloseReasons.Cancelled or CloseReasons.Duplicate or CloseReasons.NotNeeded or CloseReasons.Transferred or CloseReasons.Other)
            .WithMessage("Invalid close reason.");

        RuleFor(v => v.Explanation)
            .MaximumLength(2000).When(v => v.Explanation != null);
    }
}
