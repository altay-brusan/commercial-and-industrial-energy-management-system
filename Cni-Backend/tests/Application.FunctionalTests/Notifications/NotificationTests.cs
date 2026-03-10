using Backend.Application.Notifications.Commands.MarkAllNotificationsRead;
using Backend.Application.Notifications.Commands.MarkNotificationRead;
using Backend.Application.Notifications.Queries.GetNotifications;
using Backend.Domain.Entities;

namespace Backend.Application.FunctionalTests.Notifications;

public class NotificationTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnEmptyListWhenNoNotifications()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetNotificationsQuery());

        // Assert
        result.Should().NotBeNull();
        result.Notifications.Should().BeEmpty();
        result.UnreadCount.Should().Be(0);
    }

    [Test]
    public async Task ShouldReturnNotificationsForCurrentUser()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var notification1 = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Test Alert 1",
            Message = "First test notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        var notification2 = new Notification
        {
            UserId = userId,
            Type = "info",
            Title = "Test Alert 2",
            Message = "Second test notification",
            IsRead = true,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        await Testing.AddAsync(notification1);
        await Testing.AddAsync(notification2);

        // Act
        var result = await Testing.SendAsync(new GetNotificationsQuery());

        // Assert
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCount(2);
    }

    [Test]
    public async Task ShouldFilterByReadStatus()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var unreadNotification = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Unread Alert",
            Message = "This is unread",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        var readNotification = new Notification
        {
            UserId = userId,
            Type = "info",
            Title = "Read Alert",
            Message = "This is read",
            IsRead = true,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        await Testing.AddAsync(unreadNotification);
        await Testing.AddAsync(readNotification);

        // Act
        var result = await Testing.SendAsync(new GetNotificationsQuery { ReadStatus = "unread" });

        // Assert
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCount(1);
        result.Notifications.First().Title.Should().Be("Unread Alert");
        result.Notifications.First().Read.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReturnUnreadCount()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var unread1 = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Unread 1",
            Message = "Unread notification 1",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        var unread2 = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Unread 2",
            Message = "Unread notification 2",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        var readNotification = new Notification
        {
            UserId = userId,
            Type = "info",
            Title = "Read",
            Message = "Read notification",
            IsRead = true,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        await Testing.AddAsync(unread1);
        await Testing.AddAsync(unread2);
        await Testing.AddAsync(readNotification);

        // Act
        var result = await Testing.SendAsync(new GetNotificationsQuery());

        // Assert
        result.Should().NotBeNull();
        result.UnreadCount.Should().Be(2);
    }

    [Test]
    public async Task ShouldNotReturnOtherUsersNotifications()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var ownNotification = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "My Alert",
            Message = "My notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        // Create a second user to own a different notification
        var otherUserId = await Testing.RunAsUserAsync("other@local", "OtherUser1234!", Array.Empty<string>());
        var otherNotification = new Notification
        {
            UserId = otherUserId,
            Type = "alarm",
            Title = "Other Alert",
            Message = "Other user notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        await Testing.AddAsync(ownNotification);
        await Testing.AddAsync(otherNotification);

        // Switch back to admin user context for the query
        await Testing.RunAsAdministratorAsync();

        // Act
        var result = await Testing.SendAsync(new GetNotificationsQuery());

        // Assert
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCount(1);
        result.Notifications.First().Title.Should().Be("My Alert");
    }

    [Test]
    public async Task ShouldMarkNotificationAsRead()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var notification = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Test Alert",
            Message = "A test notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        await Testing.AddAsync(notification);

        // Act
        var result = await Testing.SendAsync(new MarkNotificationReadCommand { Id = notification.Id });

        // Assert
        result.Should().NotBeNull();
        result.Read.Should().BeTrue();

        var updated = await Testing.FindAsync<Notification>(notification.Id);
        updated.Should().NotBeNull();
        updated!.IsRead.Should().BeTrue();
    }

    [Test]
    public async Task ShouldThrowNotFoundForInvalidId()
    {
        // Arrange
        await Testing.RunAsAdministratorAsync();

        // Act & Assert
        await FluentActions.Invoking(() =>
            Testing.SendAsync(new MarkNotificationReadCommand { Id = Guid.NewGuid() }))
            .Should().ThrowAsync<Ardalis.GuardClauses.NotFoundException>();
    }

    [Test]
    public async Task ShouldMarkAllAsRead()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var notification1 = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Alert 1",
            Message = "First notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow
        };
        var notification2 = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Alert 2",
            Message = "Second notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        var notification3 = new Notification
        {
            UserId = userId,
            Type = "info",
            Title = "Alert 3",
            Message = "Third notification",
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2)
        };
        await Testing.AddAsync(notification1);
        await Testing.AddAsync(notification2);
        await Testing.AddAsync(notification3);

        // Act
        var result = await Testing.SendAsync(new MarkAllNotificationsReadCommand());

        // Assert
        result.Should().NotBeNull();
        result.UpdatedCount.Should().Be(3);

        var verifyResult = await Testing.SendAsync(new GetNotificationsQuery { ReadStatus = "unread" });
        verifyResult.Notifications.Should().BeEmpty();
        verifyResult.UnreadCount.Should().Be(0);
    }

    [Test]
    public async Task ShouldReturnZeroWhenAllAlreadyRead()
    {
        // Arrange
        var userId = await Testing.RunAsAdministratorAsync();

        var notification1 = new Notification
        {
            UserId = userId,
            Type = "alarm",
            Title = "Read Alert 1",
            Message = "Already read",
            IsRead = true,
            Timestamp = DateTimeOffset.UtcNow
        };
        var notification2 = new Notification
        {
            UserId = userId,
            Type = "info",
            Title = "Read Alert 2",
            Message = "Also already read",
            IsRead = true,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        await Testing.AddAsync(notification1);
        await Testing.AddAsync(notification2);

        // Act
        var result = await Testing.SendAsync(new MarkAllNotificationsReadCommand());

        // Assert
        result.Should().NotBeNull();
        result.UpdatedCount.Should().Be(0);
    }
}
