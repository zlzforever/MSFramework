using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class OrderStatusEntityTypeConfiguration
        : EntityTypeConfigurationBase<OrderStatus>
    {
	    public override Type DbContextType => typeof(OrderingContext);
	    
        public override void Configure(EntityTypeBuilder<OrderStatus> orderStatusConfiguration)
        {
            orderStatusConfiguration.ToTable("orderstatus", OrderingContext.DefaultSchema);

            orderStatusConfiguration.HasKey(o => o.Id);

            orderStatusConfiguration.Property(o => o.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            orderStatusConfiguration.Property(o => o.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
