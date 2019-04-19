using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class OrderItemEntityTypeConfiguration
        : EntityTypeConfigurationBase<OrderItem>
    {
	    public override Type DbContextType => typeof(OrderingContext);
	    
        public override void Configure(EntityTypeBuilder<OrderItem> orderItemConfiguration)
        {
            orderItemConfiguration.ToTable("orderItems", OrderingContext.DefaultSchema);

            orderItemConfiguration.HasKey(o => o.Id);
 
            orderItemConfiguration.Property(o => o.Id)
                .ForSqlServerUseSequenceHiLo("orderitemseq");

            orderItemConfiguration.Property<int>("OrderId")
                .IsRequired();

            orderItemConfiguration.Property<decimal>("Discount")
                .IsRequired();

            orderItemConfiguration.Property<int>("ProductId")
                .IsRequired();

            orderItemConfiguration.Property<string>("ProductName")
                .IsRequired();

            orderItemConfiguration.Property<decimal>("UnitPrice")
                .IsRequired();

            orderItemConfiguration.Property<int>("Units")
                .IsRequired();

            orderItemConfiguration.Property<string>("PictureUrl")
                .IsRequired(false);
        }
    }
}
