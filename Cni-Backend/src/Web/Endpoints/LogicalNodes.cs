using Backend.Application.LogicalNodes.Commands;
using Backend.Application.LogicalNodes.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class LogicalNodes : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/LogicalNodes")
            .WithGroupName("LogicalNodes")
            .WithTags("LogicalNodes")
            .RequireAuthorization();

        group.MapGet("/", GetLogicalNodes);
        group.MapGet("/{id}", GetLogicalNode);
        group.MapPost("/", CreateLogicalNode);
        group.MapPut("/{id}", UpdateLogicalNode);
    }

    public static async Task<Ok<List<LogicalNodeDto>>> GetLogicalNodes(
        ISender sender, string? namePrefix)
    {
        var result = await sender.Send(new GetLogicalNodesQuery { NamePrefix = namePrefix });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<LogicalNodeDto>> GetLogicalNode(
        ISender sender, Guid id)
    {
        var result = await sender.Send(new GetLogicalNodeQuery { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Created<Guid>> CreateLogicalNode(
        ISender sender, CreateLogicalNodeCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/v1/LogicalNodes/{id}", id);
    }

    public static async Task<NoContent> UpdateLogicalNode(
        ISender sender, Guid id, UpdateLogicalNodeCommand command)
    {
        if (id != command.Id)
            throw new BadHttpRequestException("Route ID does not match body ID");

        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
