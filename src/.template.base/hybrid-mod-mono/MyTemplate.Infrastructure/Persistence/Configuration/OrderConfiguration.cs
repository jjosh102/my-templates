using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTemplate.Infrastructure.Persistence.Entities;

namespace MyTemplate.Infrastructure.Persistence.Configuration;

public sealed class OrderConfiguration : IEntityTypeConfiguration<OrderRecord>
{
    public void Configure(EntityTypeBuilder<OrderRecord> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(40);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemRecord>
{
    public void Configure(EntityTypeBuilder<OrderItemRecord> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductName).HasMaxLength(200);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
    }
}
