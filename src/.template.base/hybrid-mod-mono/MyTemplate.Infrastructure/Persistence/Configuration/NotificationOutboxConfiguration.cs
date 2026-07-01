using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTemplate.Infrastructure.Persistence.Entities;

namespace MyTemplate.Infrastructure.Persistence.Configuration;

public sealed class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutboxRecord>
{
    public void Configure(EntityTypeBuilder<NotificationOutboxRecord> builder)
    {
        builder.ToTable("notification_outbox");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subject).HasMaxLength(200);
        builder.Property(x => x.Body).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
    }
}
