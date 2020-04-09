using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace MSFramework.Ef.MySql
{
	public class DescriptionPropertyAttributeConvention : PropertyAttributeConventionBase<DescriptionAttribute>
	{
		public DescriptionPropertyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies) : base(dependencies)
		{
		}

		protected override void ProcessPropertyAdded(IConventionPropertyBuilder propertyBuilder,
			DescriptionAttribute attribute,
			MemberInfo clrMember, IConventionContext context)
		{
			if (!string.IsNullOrWhiteSpace(attribute.Description))
			{
				propertyBuilder.HasComment(attribute.Description);
			}
		}
	}
}