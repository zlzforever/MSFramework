// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using MSFramework.Audit;
// using MSFramework.Ef;
//
// namespace Ordering.Infrastructure.EntityConfigurations
// {
// 	public class OperationTypeConfiguration : EntityTypeConfigurationBase<OperationType, OrderingContext>
// 	{
// 		public override void Configure(EntityTypeBuilder<OperationType> builder)
// 		{
// 			builder.HasKey(o => o.Id);
//
// 			builder.Property(o => o.Id)
// 				.HasDefaultValue(1)
// 				.ValueGeneratedNever()
// 				.IsRequired();
//
// 			builder.Property(o => o.Name)
// 				.HasMaxLength(200)
// 				.IsRequired();
// 		}
// 	}
// }