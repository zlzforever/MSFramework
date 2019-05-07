using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class OrderEntityTypeConfiguration : EntityTypeConfigurationBase<Order>
	{
		public override Type DbContextType => typeof(OrderingContext);
		
		public override void Configure(EntityTypeBuilder<Order> orderConfiguration)
		{ 
			orderConfiguration.HasKey(o => o.Id);

			//Address value object persisted as owned entity type supported since EF Core 2.0
			orderConfiguration.OwnsOne(o => o.Address);

			orderConfiguration.Property<DateTime>("CreationTime").IsRequired();
			orderConfiguration.Property<bool>("IsDeleted").IsRequired();
			orderConfiguration.Property<string>("UserId").IsRequired();
			orderConfiguration.Property<string>("Description").IsRequired(false);			

			var navigation = orderConfiguration.Metadata.FindNavigation(nameof(Order.OrderItems));
            
			// DDD Patterns comment:
			//Set as field (New since EF 1.1) to access the OrderItem collection property through its field
			navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
		}
	}
}