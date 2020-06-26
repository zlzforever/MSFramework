using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MSFramework.Domain;

namespace MSFramework.Ef.Extensions
{
	public static class PropertyBuilderExtensions
	{
		public static PropertyBuilder<TProperty> IsEnumeration<TProperty>(this PropertyBuilder<TProperty> builder)
			where TProperty : Enumeration
		{
			builder.HasConversion(new ValueConverter<TProperty, int>(
				v => v.Id,
				v => Enumeration.FromValue<TProperty>(v)));
			return builder;
		}
	}
}