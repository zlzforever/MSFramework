using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;

namespace MSFramework.Permission.Ef.EntityConfiguration
{
	public class PermissionConfiguration : EntityTypeConfigurationBase<Permission.AggregateRoot.Permission>
	{
		public override Type DbContextType => typeof(PermissionDbContext);

		public override void Configure(EntityTypeBuilder<AggregateRoot.Permission> builder)
		{
			builder.HasIndex(x => x.Hash).IsUnique();
			base.Configure(builder);
		}
	}
}