using MyTemplate.Core.Modules.Notifications.Contracts;
using MyTemplate.Core.Modules.Notifications.Domain;
using MyTemplate.Infrastructure.Persistence;
using MyTemplate.Infrastructure.Persistence.Entities;

namespace MyTemplate.Infrastructure.Notifications;

public sealed class EfNotificationOutbox(AppDbContext db) : INotificationOutbox
{
    public async Task EnqueueAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        db.NotificationOutbox.Add(new NotificationOutboxRecord
        {
            Id = Guid.NewGuid(),
            RecipientId = message.RecipientId,
            Subject = message.Subject,
            Body = message.Body,
            RequestedAt = message.RequestedAt,
            Status = NotificationOutboxStatus.Pending
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
