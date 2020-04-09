using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace MSFramework.Ef.MySql
{
	public class MySqlConventionSetBuilder
		: Microsoft.EntityFrameworkCore.Metadata.Conventions.MySqlConventionSetBuilder
	{
		public MySqlConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies,
			RelationalConventionSetBuilderDependencies relationalDependencies) : base(dependencies,
			relationalDependencies)
		{
		}

		public override ConventionSet CreateConventionSet()
		{
			var conventionSet = base.CreateConventionSet();
			var descriptionEntityTypeAttributeConvention = new DescriptionEntityTypeAttributeConvention(Dependencies);
			conventionSet.EntityTypeAddedConventions.Add(descriptionEntityTypeAttributeConvention);

			var descriptionConvention = new DescriptionPropertyAttributeConvention(Dependencies);
			conventionSet.PropertyAddedConventions.Add(descriptionConvention);
			return conventionSet;
		}
	}
}