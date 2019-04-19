using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Buyer;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class BuyerEntityTypeConfiguration
		: EntityTypeConfigurationBase<Buyer>
	{
		public override Type DbContextType => typeof(OrderingContext);

		public override void Configure(EntityTypeBuilder<Buyer> buyerConfiguration)
		{
			buyerConfiguration.ToTable("buyers", OrderingContext.DefaultSchema);

			buyerConfiguration.HasKey(b => b.Id);

			buyerConfiguration.Property(b => b.Id)
				.ForSqlServerUseSequenceHiLo("buyerseq", OrderingContext.DefaultSchema);

			buyerConfiguration.Property(b => b.Identity)
				.HasMaxLength(200)
				.IsRequired();

			buyerConfiguration.HasIndex("IdentityGuid")
				.IsUnique(true);

			buyerConfiguration.Property(b => b.Name);

			buyerConfiguration.HasMany(b => b.PaymentMethods)
				.WithOne()
				.HasForeignKey("BuyerId")
				.OnDelete(DeleteBehavior.Cascade);

			var navigation = buyerConfiguration.Metadata.FindNavigation(nameof(Buyer.PaymentMethods));

			navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
		}
	}
}