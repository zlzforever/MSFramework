using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Buyer;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class OrderEntityTypeConfiguration : EntityTypeConfigurationBase<Order>
	{
		public override Type DbContextType => typeof(OrderingContext);
		
		public override void Configure(EntityTypeBuilder<Order> orderConfiguration)
		{
			orderConfiguration.ToTable("orders", OrderingContext.DefaultSchema);

			orderConfiguration.HasKey(o => o.Id);

			orderConfiguration.Property(o => o.Id)
				.ForSqlServerUseSequenceHiLo("orderseq", OrderingContext.DefaultSchema);

			//Address value object persisted as owned entity type supported since EF Core 2.0
			orderConfiguration.OwnsOne(o => o.Address);

			orderConfiguration.Property<DateTime>("OrderDate").IsRequired();
			orderConfiguration.Property<int?>("BuyerId").IsRequired(false);
			orderConfiguration.Property<int>("OrderStatusId").IsRequired();
			orderConfiguration.Property<int?>("PaymentMethodId").IsRequired(false);
			orderConfiguration.Property<string>("Description").IsRequired(false);

			var navigation = orderConfiguration.Metadata.FindNavigation(nameof(Order.OrderItems));

			// DDD Patterns comment:
			//Set as field (New since EF 1.1) to access the OrderItem collection property through its field
			navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

			orderConfiguration.HasOne<PaymentMethod>()
				.WithMany()
				.HasForeignKey("PaymentMethodId")
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);

			orderConfiguration.HasOne<Buyer>()
				.WithMany()
				.IsRequired(false)
				.HasForeignKey("BuyerId");

			orderConfiguration.HasOne(o => o.OrderStatus)
				.WithMany()
				.HasForeignKey("OrderStatusId");
		}
	}
}