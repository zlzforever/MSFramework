using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Buyer;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class PaymentMethodEntityTypeConfiguration
		: EntityTypeConfigurationBase<PaymentMethod>
	{
		public override Type DbContextType => typeof(OrderingContext);

		public override void Configure(EntityTypeBuilder<PaymentMethod> paymentConfiguration)
		{
			paymentConfiguration.ToTable("paymentmethods", OrderingContext.DefaultSchema);

			paymentConfiguration.HasKey(b => b.Id);

			paymentConfiguration.Property(b => b.Id)
				.ForSqlServerUseSequenceHiLo("paymentseq", OrderingContext.DefaultSchema);

			paymentConfiguration.Property<int>("BuyerId")
				.IsRequired();

			paymentConfiguration.Property<string>("CardHolderName")
				.HasMaxLength(200)
				.IsRequired();

			paymentConfiguration.Property<string>("Alias")
				.HasMaxLength(200)
				.IsRequired();

			paymentConfiguration.Property<string>("CardNumber")
				.HasMaxLength(25)
				.IsRequired();

			paymentConfiguration.Property<DateTime>("Expiration")
				.IsRequired();

			paymentConfiguration.Property<int>("CardTypeId")
				.IsRequired();

			paymentConfiguration.HasOne(p => p.CardType)
				.WithMany()
				.HasForeignKey("CardTypeId");
		}
	}
}