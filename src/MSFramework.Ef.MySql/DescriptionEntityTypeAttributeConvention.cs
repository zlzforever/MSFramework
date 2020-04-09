using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace MSFramework.Ef.MySql
{
	public class DescriptionEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<DescriptionAttribute>
	{
		public DescriptionEntityTypeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies) : base(
			dependencies)
		{
		}

		protected override void ProcessEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder,
			DescriptionAttribute attribute,
			IConventionContext<IConventionEntityTypeBuilder> context)
		{
			if (!string.IsNullOrWhiteSpace(attribute.Description))
			{
				entityTypeBuilder.HasComment(attribute.Description);
			}
		}
	}
}