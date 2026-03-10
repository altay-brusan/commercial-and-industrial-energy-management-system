using Backend.Application.Common.Exceptions;
using Backend.Application.Maintenance.Commands.AddChecklistItem;
using Backend.Application.Maintenance.Commands.AddTaskNote;
using Backend.Application.Maintenance.Commands.CloseMaintenanceTask;
using Backend.Application.Maintenance.Commands.CompleteMaintenanceTask;
using Backend.Application.Maintenance.Commands.CreateMaintenanceTask;
using Backend.Application.Maintenance.Commands.UpdateMaintenanceTaskStatus;
using Backend.Application.Maintenance.Queries.GetMaintenanceTask;
using Backend.Application.Maintenance.Queries.GetMaintenanceTasks;
using Backend.Domain.Constants;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.FunctionalTests.Maintenance;

using static Testing;

public class MaintenanceTaskTests : BaseTestFixture
{
    private Guid _facilityId;
    private string _userId = null!;

    private async Task SetupTestData()
    {
        _userId = await RunAsAdministratorAsync();
        var tenant = new Tenant { Name = "Test Tenant" };
        await AddAsync(tenant);
        var firm = new Firm { TenantId = tenant.Id, Name = "Test Firm", Type = "Standard", Country = "Netherlands", State = "Noord-Holland", City = "Amsterdam", Address = "Test 1", ContactEmail = "test@test.com", ContactPhone = "+31000000000", TaxNumber = "NL000000000B01", TaxOffice = "Amsterdam" };
        await AddAsync(firm);
        var facility = new Facility { FirmId = firm.Id, Name = "Test Facility", EnergyCapacity = 100, EnergyUnit = EnergyUnit.KWh };
        await AddAsync(facility);
        _facilityId = facility.Id;
    }

    private CreateMaintenanceTaskCommand CreateValidCommand(DateTimeOffset? dueDate = null, List<string>? checklistItems = null)
    {
        return new CreateMaintenanceTaskCommand
        {
            Title = "Replace faulty inverter",
            Description = "Inverter #3 showing error codes",
            AssignedToId = _userId,
            Priority = TaskPriorities.High,
            DueDate = dueDate ?? DateTimeOffset.UtcNow.AddDays(7),
            FacilityId = _facilityId,
            ChecklistItems = checklistItems
        };
    }

    #region CreateMaintenanceTask

    [Test]
    public async Task ShouldCreateTask()
    {
        await SetupTestData();

        var command = CreateValidCommand();

        var taskId = await SendAsync(command);

        var task = await FindAsync<MaintenanceTask>(taskId);
        task.Should().NotBeNull();
        task!.Title.Should().Be("Replace faulty inverter");
        task.Description.Should().Be("Inverter #3 showing error codes");
        task.Priority.Should().Be(TaskPriorities.High);
        task.Status.Should().Be(TaskStatuses.Scheduled);
        task.AssignedToId.Should().Be(_userId);
        task.AssignedById.Should().Be(_userId);
        task.FacilityId.Should().Be(_facilityId);
    }

    [Test]
    public async Task ShouldSetPendingStatusWhenDueDateIsPast()
    {
        await SetupTestData();

        var command = CreateValidCommand(dueDate: DateTimeOffset.UtcNow.AddDays(-1));

        var taskId = await SendAsync(command);

        var task = await FindAsync<MaintenanceTask>(taskId);
        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatuses.Pending);
    }

    [Test]
    public async Task ShouldCreateWithChecklistItems()
    {
        await SetupTestData();

        var items = new List<string> { "Disconnect power", "Remove old inverter", "Install new inverter" };
        var command = CreateValidCommand(checklistItems: items);

        var taskId = await SendAsync(command);

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });
        detail.Checklist.Should().HaveCount(3);
        detail.Checklist[0].Label.Should().Be("Disconnect power");
        detail.Checklist[0].SortOrder.Should().Be(0);
        detail.Checklist[1].Label.Should().Be("Remove old inverter");
        detail.Checklist[1].SortOrder.Should().Be(1);
        detail.Checklist[2].Label.Should().Be("Install new inverter");
        detail.Checklist[2].SortOrder.Should().Be(2);
    }

    [Test]
    public async Task ShouldHaveInitialStatusChange()
    {
        await SetupTestData();

        var command = CreateValidCommand();

        var taskId = await SendAsync(command);

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });
        detail.Timeline.Should().HaveCount(1);
        detail.Timeline[0].FromStatus.Should().BeNull();
        detail.Timeline[0].ToStatus.Should().Be(TaskStatuses.Scheduled);
    }

    [Test]
    public async Task ShouldRequireTitle()
    {
        await SetupTestData();

        var command = CreateValidCommand() with { Title = "" };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireFacilityId()
    {
        await SetupTestData();

        var command = CreateValidCommand() with { FacilityId = Guid.Empty };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireAssignedToId()
    {
        await SetupTestData();

        var command = CreateValidCommand() with { AssignedToId = "" };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireValidPriority()
    {
        await SetupTestData();

        var command = CreateValidCommand() with { Priority = "InvalidPriority" };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireDueDate()
    {
        await SetupTestData();

        var command = CreateValidCommand() with { DueDate = default };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GetMaintenanceTasks

    [Test]
    public async Task ShouldReturnAllTasks()
    {
        await SetupTestData();

        await SendAsync(CreateValidCommand() with { Title = "Task 1" });
        await SendAsync(CreateValidCommand() with { Title = "Task 2" });
        await SendAsync(CreateValidCommand() with { Title = "Task 3" });

        var result = await SendAsync(new GetMaintenanceTasksQuery());

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Test]
    public async Task ShouldFilterByStatus()
    {
        await SetupTestData();

        await SendAsync(CreateValidCommand(dueDate: DateTimeOffset.UtcNow.AddDays(7)) with { Title = "Future task" });
        await SendAsync(CreateValidCommand(dueDate: DateTimeOffset.UtcNow.AddDays(-1)) with { Title = "Past task" });

        var result = await SendAsync(new GetMaintenanceTasksQuery { Status = TaskStatuses.Scheduled });

        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Future task");
    }

    [Test]
    public async Task ShouldFilterByPriority()
    {
        await SetupTestData();

        await SendAsync(CreateValidCommand() with { Priority = TaskPriorities.Critical, Title = "Critical task" });
        await SendAsync(CreateValidCommand() with { Priority = TaskPriorities.Low, Title = "Low task" });

        var result = await SendAsync(new GetMaintenanceTasksQuery { Priority = TaskPriorities.Critical });

        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Critical task");
    }

    [Test]
    public async Task ShouldFilterByFacilityId()
    {
        await SetupTestData();

        // Create a second facility
        var tenant2 = new Tenant { Name = "Tenant 2" };
        await AddAsync(tenant2);
        var firm2 = new Firm { TenantId = tenant2.Id, Name = "Firm 2", Type = "Standard", Country = "Netherlands", State = "Noord-Holland", City = "Amsterdam", Address = "Test 2", ContactEmail = "test2@test.com", ContactPhone = "+31000000001", TaxNumber = "NL000000001B01", TaxOffice = "Amsterdam" };
        await AddAsync(firm2);
        var facility2 = new Facility { FirmId = firm2.Id, Name = "Facility 2", EnergyCapacity = 200, EnergyUnit = EnergyUnit.KWh };
        await AddAsync(facility2);

        await SendAsync(CreateValidCommand() with { Title = "Task at facility 1" });
        await SendAsync(CreateValidCommand() with { Title = "Task at facility 2", FacilityId = facility2.Id });

        var result = await SendAsync(new GetMaintenanceTasksQuery { FacilityId = _facilityId });

        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Task at facility 1");
    }

    [Test]
    public async Task ShouldPaginate()
    {
        await SetupTestData();

        for (var i = 0; i < 5; i++)
        {
            await SendAsync(CreateValidCommand() with { Title = $"Task {i}" });
        }

        var result = await SendAsync(new GetMaintenanceTasksQuery { PageSize = 2, PageNumber = 1 });

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    #endregion

    #region GetMaintenanceTask

    [Test]
    public async Task ShouldReturnTaskDetail()
    {
        await SetupTestData();

        var command = CreateValidCommand(checklistItems: new List<string> { "Step 1" });
        var taskId = await SendAsync(command);

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });

        detail.Should().NotBeNull();
        detail.Id.Should().Be(taskId);
        detail.Title.Should().Be("Replace faulty inverter");
        detail.Description.Should().Be("Inverter #3 showing error codes");
        detail.Status.Should().Be(TaskStatuses.Scheduled);
        detail.Priority.Should().Be(TaskPriorities.High);
        detail.FacilityId.Should().Be(_facilityId);
        detail.FacilityName.Should().Be("Test Facility");
        detail.Checklist.Should().HaveCount(1);
        detail.Timeline.Should().HaveCount(1);
        detail.Notes.Should().BeEmpty();
        detail.WorkSessions.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldThrowNotFoundForInvalidId()
    {
        await SetupTestData();

        await FluentActions.Invoking(() =>
            SendAsync(new GetMaintenanceTaskQuery { Id = Guid.NewGuid() }))
            .Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateMaintenanceTaskStatus

    [Test]
    public async Task ShouldTransitionScheduledToPending()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());

        await SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = taskId, NewStatus = TaskStatuses.Pending });

        var task = await FindAsync<MaintenanceTask>(taskId);
        task!.Status.Should().Be(TaskStatuses.Pending);
    }

    [Test]
    public async Task ShouldTransitionPendingToInProgress()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand(dueDate: DateTimeOffset.UtcNow.AddDays(-1)));
        // Created as Pending because DueDate is in the past

        await SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = taskId, NewStatus = TaskStatuses.InProgress });

        var task = await FindAsync<MaintenanceTask>(taskId);
        task!.Status.Should().Be(TaskStatuses.InProgress);
    }

    [Test]
    public async Task ShouldRejectInvalidTransition()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());
        // Status is Scheduled

        await FluentActions.Invoking(() =>
            SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = taskId, NewStatus = TaskStatuses.Completed }))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldThrowNotFoundForInvalidTask()
    {
        await SetupTestData();

        await FluentActions.Invoking(() =>
            SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = Guid.NewGuid(), NewStatus = TaskStatuses.Pending }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRecordStatusChangeInTimeline()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());
        await SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = taskId, NewStatus = TaskStatuses.Pending });

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });

        detail.Timeline.Should().HaveCount(2);
        detail.Timeline.Should().Contain(s => s.FromStatus == null && s.ToStatus == TaskStatuses.Scheduled);
        detail.Timeline.Should().Contain(s => s.FromStatus == TaskStatuses.Scheduled && s.ToStatus == TaskStatuses.Pending);
    }

    #endregion

    #region CompleteMaintenanceTask

    [Test]
    public async Task ShouldCompleteInProgressTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand(dueDate: DateTimeOffset.UtcNow.AddDays(-1)));
        await SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = taskId, NewStatus = TaskStatuses.InProgress });

        await SendAsync(new CompleteMaintenanceTaskCommand
        {
            Id = taskId,
            Resolution = "Replaced inverter module",
            RootCause = "Manufacturing defect",
            LaborHours = 4.5m,
            PartsUsed = "Inverter module XYZ-100",
            Recommendations = "Schedule quarterly inspection"
        });

        var task = await FindAsync<MaintenanceTask>(taskId);
        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatuses.Completed);
        task.Resolution.Should().Be("Replaced inverter module");
        task.RootCause.Should().Be("Manufacturing defect");
        task.LaborHours.Should().Be(4.5m);
        task.PartsUsed.Should().Be("Inverter module XYZ-100");
        task.Recommendations.Should().Be("Schedule quarterly inspection");
        task.CompletedAt.Should().NotBeNull();
        task.CompletedById.Should().Be(_userId);
    }

    [Test]
    public async Task ShouldRejectCompletionOfScheduledTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());
        // Status is Scheduled

        await FluentActions.Invoking(() =>
            SendAsync(new CompleteMaintenanceTaskCommand { Id = taskId, Resolution = "Done" }))
            .Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CloseMaintenanceTask

    [Test]
    public async Task ShouldCloseScheduledTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());

        await SendAsync(new CloseMaintenanceTaskCommand
        {
            Id = taskId,
            Reason = CloseReasons.NotNeeded,
            Explanation = "Issue resolved itself"
        });

        var task = await FindAsync<MaintenanceTask>(taskId);
        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatuses.Closed);
        task.CloseReason.Should().Be(CloseReasons.NotNeeded);
        task.CloseExplanation.Should().Be("Issue resolved itself");
        task.ClosedAt.Should().NotBeNull();
        task.ClosedById.Should().Be(_userId);
    }

    [Test]
    public async Task ShouldRejectClosingAlreadyClosedTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());
        await SendAsync(new CloseMaintenanceTaskCommand { Id = taskId, Reason = CloseReasons.Cancelled });

        await FluentActions.Invoking(() =>
            SendAsync(new CloseMaintenanceTaskCommand { Id = taskId, Reason = CloseReasons.Duplicate }))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRejectClosingCompletedTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand(dueDate: DateTimeOffset.UtcNow.AddDays(-1)));
        await SendAsync(new UpdateMaintenanceTaskStatusCommand { Id = taskId, NewStatus = TaskStatuses.InProgress });
        await SendAsync(new CompleteMaintenanceTaskCommand { Id = taskId, Resolution = "Fixed" });

        await FluentActions.Invoking(() =>
            SendAsync(new CloseMaintenanceTaskCommand { Id = taskId, Reason = CloseReasons.Other }))
            .Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region AddTaskNote

    [Test]
    public async Task ShouldAddNoteToTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());

        var noteId = await SendAsync(new AddTaskNoteCommand
        {
            TaskId = taskId,
            Content = "Ordered replacement parts"
        });

        noteId.Should().NotBe(Guid.Empty);

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });
        detail.Notes.Should().HaveCount(1);
        detail.Notes[0].Content.Should().Be("Ordered replacement parts");
    }

    #endregion

    #region AddChecklistItem

    [Test]
    public async Task ShouldAddChecklistItemToTask()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand(checklistItems: new List<string> { "Existing item" }));

        var itemId = await SendAsync(new AddChecklistItemCommand
        {
            TaskId = taskId,
            Label = "New checklist item"
        });

        itemId.Should().NotBe(Guid.Empty);

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });
        detail.Checklist.Should().HaveCount(2);
        detail.Checklist[0].Label.Should().Be("Existing item");
        detail.Checklist[0].SortOrder.Should().Be(0);
        detail.Checklist[1].Label.Should().Be("New checklist item");
        detail.Checklist[1].SortOrder.Should().Be(1);
    }

    [Test]
    public async Task ShouldAddChecklistItemToEmptyList()
    {
        await SetupTestData();

        var taskId = await SendAsync(CreateValidCommand());

        var itemId = await SendAsync(new AddChecklistItemCommand
        {
            TaskId = taskId,
            Label = "First item"
        });

        itemId.Should().NotBe(Guid.Empty);

        var detail = await SendAsync(new GetMaintenanceTaskQuery { Id = taskId });
        detail.Checklist.Should().HaveCount(1);
        detail.Checklist[0].Label.Should().Be("First item");
        detail.Checklist[0].SortOrder.Should().Be(0);
    }

    #endregion
}
