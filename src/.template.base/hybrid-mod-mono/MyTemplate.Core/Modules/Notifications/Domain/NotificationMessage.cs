namespace MyTemplate.Core.Modules.Notifications.Domain;

public sealed record NotificationMessage(
    Guid RecipientId,
    string Subject,
    string Body,
    DateTimeOffset RequestedAt);
