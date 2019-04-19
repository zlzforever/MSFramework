using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.EntityFrameworkCore;
using Ordering.Domain.AggregateRoot.Buyer;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class CardTypeEntityTypeConfiguration
	    : EntityTypeConfigurationBase<CardType>
    {
	    public override Type DbContextType => typeof(OrderingContext);
	    
        public override void Configure(EntityTypeBuilder<CardType> cardTypesConfiguration)
        {
            cardTypesConfiguration.ToTable("cardtypes", OrderingContext.DefaultSchema);

            cardTypesConfiguration.HasKey(ct => ct.Id);

            cardTypesConfiguration.Property(ct => ct.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            cardTypesConfiguration.Property(ct => ct.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
