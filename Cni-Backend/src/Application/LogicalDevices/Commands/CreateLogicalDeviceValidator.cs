namespace Backend.Application.LogicalDevices.Commands;

public class CreateLogicalDeviceCommandValidator : AbstractValidator<CreateLogicalDeviceCommand>
{
    public CreateLogicalDeviceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DisplayName)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Ports)
            .NotEmpty();

        RuleForEach(x => x.NodeAssignments)
            .ChildRules(a =>
            {
                a.RuleFor(n => n.LogicalNodeId).NotEmpty();
                a.RuleFor(n => n.InstanceCount).GreaterThan(0);
                a.RuleFor(n => n.NodeAlias).MaximumLength(100);
            });
    }
}
