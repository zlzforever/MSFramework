using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class OrderItemEntityTypeConfiguration : EntityTypeConfigurationBase<OrderItem, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<OrderItem> builder)
		{
			base.Configure(builder);

			builder.HasKey(o => o.Id);

			builder.Property<decimal>("Discount")
				.IsRequired();

			builder.Property<Guid>("ProductId")
				.IsRequired();

			builder.Property<string>("ProductName")
				.IsRequired();

			builder.Property<decimal>("UnitPrice")
				.IsRequired();

			builder.Property<int>("Units")
				.IsRequired();

			builder.Property<string>("PictureUrl")
				.IsRequired(false);
		}
	}
}