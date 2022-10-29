// using MicroserviceFramework.Auditing;
// using MicroserviceFramework.Ef;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
//
// namespace Ordering.Infrastructure.EntityConfigurations.Auditing;
//
// public class AuditPropertyConfiguration
//     : EntityTypeConfigurationBase<AuditProperty, OrderingContext>
// {
//     public override void Configure(EntityTypeBuilder<AuditProperty> builder)
//     {
//         builder.Property(x => x.Id).ValueGeneratedNever().HasMaxLength(36);
//         builder.Property(x => x.Name).HasMaxLength(255);
//         builder.Property(x => x.Type).HasMaxLength(255);
//         builder.Property(x => x.NewValue);
//         builder.Property(x => x.OriginalValue);
//     }
// }
