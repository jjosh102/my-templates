using MyTemplate.Core.Modules.Notifications.Domain;

namespace MyTemplate.Core.Modules.Notifications.Contracts;

public interface INotificationOutbox
{
    Task EnqueueAsync(NotificationMessage message, CancellationToken cancellationToken);
}
