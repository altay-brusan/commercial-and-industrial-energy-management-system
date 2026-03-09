using Backend.Domain.Constants;

namespace Backend.Application.Maintenance.Commands.CreateMaintenanceTask;

public class CreateMaintenanceTaskCommandValidator : AbstractValidator<CreateMaintenanceTaskCommand>
{
    public CreateMaintenanceTaskCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.AssignedToId)
            .NotEmpty().WithMessage("Assigned user is required.");

        RuleFor(v => v.FacilityId)
            .NotEmpty().WithMessage("Facility is required.");

        RuleFor(v => v.DueDate)
            .NotEmpty().WithMessage("Due date is required.");

        RuleFor(v => v.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(p => p is TaskPriorities.Critical or TaskPriorities.High or TaskPriorities.Medium or TaskPriorities.Low)
            .WithMessage("Priority must be Critical, High, Medium, or Low.");

        RuleFor(v => v.Description)
            .MaximumLength(2000).When(v => v.Description != null);
    }
}
