using Microsoft.EntityFrameworkCore;
using MyTemplate.Infrastructure.Persistence.Entities;

namespace MyTemplate.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OrderRecord> Orders => Set<OrderRecord>();
    public DbSet<OrderItemRecord> OrderItems => Set<OrderItemRecord>();
    public DbSet<NotificationOutboxRecord> NotificationOutbox => Set<NotificationOutboxRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
