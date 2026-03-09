using Backend.Application.Notifications.Commands.MarkAllNotificationsRead;
using Backend.Application.Notifications.Commands.MarkNotificationRead;
using Backend.Application.Notifications.Queries;
using Backend.Application.Notifications.Queries.GetNotifications;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Web.Endpoints;

public class Notifications : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/Notifications")
            .WithGroupName("Notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", GetNotifications);
        group.MapPatch("/{id}/Read", MarkAsRead);
        group.MapPatch("/MarkAllRead", MarkAllRead);
    }

    public static async Task<Ok<NotificationListResponseDto>> GetNotifications(
        ISender sender, string? readStatus, string? type)
    {
        var result = await sender.Send(new GetNotificationsQuery
        {
            ReadStatus = readStatus,
            Type = type
        });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<NotificationDto>> MarkAsRead(ISender sender, Guid id)
    {
        var result = await sender.Send(new MarkNotificationReadCommand { Id = id });
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<MarkAllReadResponseDto>> MarkAllRead(ISender sender)
    {
        var result = await sender.Send(new MarkAllNotificationsReadCommand());
        return TypedResults.Ok(result);
    }
}
