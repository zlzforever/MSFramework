using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using Template.Domain.AggregateRoot;

namespace Template.Infrastructure.EntityConfiguration
{
	public class Class1Configuration : EntityTypeConfigurationBase<Class1>
	{
		public override Type DbContextType => typeof(AppDbContext);

		public override void Configure(EntityTypeBuilder<Class1> builder)
		{
			base.Configure(builder);

			builder.HasIndex(x => x.Name).IsUnique();
		}
	}
}