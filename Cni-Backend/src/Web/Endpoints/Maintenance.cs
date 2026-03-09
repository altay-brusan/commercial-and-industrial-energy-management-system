using Backend.Application.Common.Models;
using Backend.Application.Maintenance.Commands.AddChecklistItem;
using Backend.Application.Maintenance.Commands.AddTaskNote;
using Backend.Application.Maintenance.Commands.CloseMaintenanceTask;
using Backend.Application.Maintenance.Commands.CompleteMaintenanceTask;
using Backend.Application.Maintenance.Commands.CreateMaintenanceTask;
using Backend.Application.Maintenance.Commands.StartWorkSession;
using Backend.Application.Maintenance.Commands.StopWorkSession;
using Backend.Application.Maintenance.Commands.UpdateChecklistItem;
using Backend.Application.Maintenance.Commands.UpdateMaintenanceTaskStatus;
using Backend.Application.Maintenance.Queries;
using Backend.Application.Maintenance.Queries.GetMaintenanceTask;
using Backend.Application.Maintenance.Queries.GetMaintenanceTasks;
using Backend.Application.Maintenance.Queries.GetScheduledMaintenances;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Maintenance : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Maintenance")
            .WithGroupName("Maintenance")
            .WithTags("Maintenance")
            .RequireAuthorization();

        group.MapGet("/Tasks", GetTasks);
        group.MapGet("/Tasks/Calendar", GetCalendar);
        group.MapGet("/Tasks/{id}", GetTask);
        group.MapPost("/Tasks", CreateTask);
        group.MapPost("/Tasks/{id}/Status", UpdateStatus);
        group.MapPost("/Tasks/{id}/Complete", CompleteTask);
        group.MapPost("/Tasks/{id}/Close", CloseTask);
        group.MapPost("/Tasks/{id}/Notes", AddNote);
        group.MapPost("/Tasks/{id}/Checklist", AddChecklistItem);
        group.MapPut("/Tasks/{id}/Checklist/{itemId}", UpdateChecklistItem);
        group.MapPost("/Tasks/{id}/WorkSessions/Start", StartWorkSession);
        group.MapPost("/Tasks/{id}/WorkSessions/Stop", StopWorkSession);
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
        return TypedResults.Created($"/api/v1/Maintenance/Tasks/{id}", id);
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
        return TypedResults.Created($"/api/v1/Maintenance/Tasks/{id}/Notes/{noteId}", noteId);
    }

    public static async Task<Created<Guid>> AddChecklistItem(ISender sender, Guid id, AddChecklistItemCommand command)
    {
        var itemId = await sender.Send(command with { TaskId = id });
        return TypedResults.Created($"/api/v1/Maintenance/Tasks/{id}/Checklist/{itemId}", itemId);
    }

    public static async Task<NoContent> UpdateChecklistItem(ISender sender, Guid id, Guid itemId, UpdateChecklistItemCommand command)
    {
        await sender.Send(command with { TaskId = id, ChecklistItemId = itemId });
        return TypedResults.NoContent();
    }

    public static async Task<Created<Guid>> StartWorkSession(ISender sender, Guid id)
    {
        var sessionId = await sender.Send(new StartWorkSessionCommand { TaskId = id });
        return TypedResults.Created($"/api/v1/Maintenance/Tasks/{id}/WorkSessions/{sessionId}", sessionId);
    }

    public static async Task<NoContent> StopWorkSession(ISender sender, Guid id)
    {
        await sender.Send(new StopWorkSessionCommand { TaskId = id });
        return TypedResults.NoContent();
    }
}
