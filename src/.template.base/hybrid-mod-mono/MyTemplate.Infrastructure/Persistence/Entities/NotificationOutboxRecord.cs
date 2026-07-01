namespace MyTemplate.Infrastructure.Persistence.Entities;

public sealed class NotificationOutboxRecord
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public NotificationOutboxStatus Status { get; set; }
}

public enum NotificationOutboxStatus
{
    Pending,
    Processed,
    Failed
}
