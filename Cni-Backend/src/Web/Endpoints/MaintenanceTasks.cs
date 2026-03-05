using Backend.Application.Common.Models;
using Backend.Application.MaintenanceTasks.Commands.AddChecklistItem;
using Backend.Application.MaintenanceTasks.Commands.AddTaskNote;
using Backend.Application.MaintenanceTasks.Commands.CloseMaintenanceTask;
using Backend.Application.MaintenanceTasks.Commands.CompleteMaintenanceTask;
using Backend.Application.MaintenanceTasks.Commands.CreateMaintenanceTask;
using Backend.Application.MaintenanceTasks.Commands.StartWorkSession;
using Backend.Application.MaintenanceTasks.Commands.StopWorkSession;
using Backend.Application.MaintenanceTasks.Commands.UpdateChecklistItem;
using Backend.Application.MaintenanceTasks.Commands.UpdateMaintenanceTaskStatus;
using Backend.Application.MaintenanceTasks.Queries;
using Backend.Application.MaintenanceTasks.Queries.GetMaintenanceTask;
using Backend.Application.MaintenanceTasks.Queries.GetMaintenanceTasks;
using Backend.Application.MaintenanceTasks.Queries.GetScheduledMaintenances;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class MaintenanceTasks : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/MaintenanceTasks")
            .WithGroupName("MaintenanceTasks")
            .WithTags("MaintenanceTasks")
            .RequireAuthorization();

        group.MapGet("/", GetTasks);
        group.MapGet("/Calendar", GetCalendar);
        group.MapGet("/{id}", GetTask);
        group.MapPost("/", CreateTask);
        group.MapPost("/{id}/Status", UpdateStatus);
        group.MapPost("/{id}/Complete", CompleteTask);
        group.MapPost("/{id}/Close", CloseTask);
        group.MapPost("/{id}/Notes", AddNote);
        group.MapPost("/{id}/Checklist", AddChecklistItem);
        group.MapPut("/{id}/Checklist/{itemId}", UpdateChecklistItem);
        group.MapPost("/{id}/WorkSessions/Start", StartWorkSession);
        group.MapPost("/{id}/WorkSessions/Stop", StopWorkSession);
    }

    public static async Task<Ok<PaginatedList<MaintenanceTaskBriefDto>>> GetTasks(
        ISender sender,
        Guid? facilityId, string? status, string? priority, string? assignedToId,
        DateTimeOffset? dateFrom, DateTimeOffset? dateTo, string? searchTerm,
        int? pageNumber, int? pageSize)
    {
        var result = await sender.Send(new GetMaintenanceTasksQuery
        {
            FacilityId = facilityId,
            Status = status,
            Priority = priority,
            AssignedToId = assignedToId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            SearchTerm = searchTerm,
            PageNumber = pageNumber ?? 1,
            PageSize = pageSize ?? 20
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<List<MaintenanceTaskBriefDto>>> GetCalendar(
        ISender sender, int month, int year, Guid? facilityId)
    {
        var result = await sender.Send(new GetScheduledMaintenancesQuery
        {
            Month = month,
            Year = year,
            FacilityId = facilityId
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<MaintenanceTaskDetailDto>> GetTask(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetMaintenanceTaskQuery { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateTask(ISender sender, CreateMaintenanceTaskCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/v1/MaintenanceTasks/{id}", id);
    }

    public static async Task<NoContent> UpdateStatus(ISender sender, Guid id, UpdateMaintenanceTaskStatusCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CompleteTask(ISender sender, Guid id, CompleteMaintenanceTaskCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CloseTask(ISender sender, Guid id, CloseMaintenanceTaskCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<Created<Guid>> AddNote(ISender sender, Guid id, AddTaskNoteCommand command)
    {
        var noteId = await sender.Send(command with { TaskId = id });
        return TypedResults.Created($"/api/v1/MaintenanceTasks/{id}/Notes/{noteId}", noteId);
    }

    public static async Task<Created<Guid>> AddChecklistItem(ISender sender, Guid id, AddChecklistItemCommand command)
    {
        var itemId = await sender.Send(command with { TaskId = id });
        return TypedResults.Created($"/api/v1/MaintenanceTasks/{id}/Checklist/{itemId}", itemId);
    }

    public static async Task<NoContent> UpdateChecklistItem(ISender sender, Guid id, Guid itemId, UpdateChecklistItemCommand command)
    {
        await sender.Send(command with { TaskId = id, ChecklistItemId = itemId });
        return TypedResults.NoContent();
    }

    public static async Task<Created<Guid>> StartWorkSession(ISender sender, Guid id)
    {
        var sessionId = await sender.Send(new StartWorkSessionCommand { TaskId = id });
        return TypedResults.Created($"/api/v1/MaintenanceTasks/{id}/WorkSessions/{sessionId}", sessionId);
    }

    public static async Task<NoContent> StopWorkSession(ISender sender, Guid id)
    {
        await sender.Send(new StopWorkSessionCommand { TaskId = id });
        return TypedResults.NoContent();
    }
}
