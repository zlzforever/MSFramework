using MicroserviceFramework.Ef.Infrastructure;
using MicroserviceFramework.Extensions;
using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Extensions
{
	public static class ModelBuilderExtensions
	{
		public static ModelBuilder UseObjectId(this ModelBuilder modelBuilder)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var properties = entityType.GetProperties();
				foreach (var property in properties)
				{
					if (property.ClrType == typeof(ObjectId))
					{
						property.SetValueConverter(new ObjectIdToStringConverter());
					}
				}
			}

			return modelBuilder;
		}

		public static ModelBuilder UseUnderScoreCase(this ModelBuilder modelBuilder)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				if (!entityType.IsOwned())
				{
					var tableName = entityType.GetTableName();
					entityType.SetTableName(tableName.ToUnderScoreCase());
				}

				var properties = entityType.GetProperties();
				foreach (var property in properties)
				{
					var propertyName = property.GetColumnName();
					if (propertyName.StartsWith("_"))
					{
						propertyName = propertyName.Substring(1, propertyName.Length - 1);
					}

					property.SetColumnName(propertyName.ToUnderScoreCase());
				}
			}

			return modelBuilder;
		}
	}
}