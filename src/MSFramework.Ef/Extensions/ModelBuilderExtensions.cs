using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MSFramework.Ef.Infrastructure;
using MSFramework.Shared;
using MSFramework.Utilities;

namespace MSFramework.Ef.Extensions
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

		public static ModelBuilder UseUnixLikeName(this ModelBuilder modelBuilder)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				if (!entityType.IsOwned())
				{
					var tableName = entityType.GetTableName();
					entityType.SetTableName(StringUtilities.ToUnixLike(tableName));
				}

				var properties = entityType.GetProperties();
				foreach (var property in properties)
				{
					var propertyName = property.GetColumnName();
					if (propertyName.StartsWith("_"))
					{
						propertyName = propertyName.Substring(1, propertyName.Length - 1);
					}

					property.SetColumnName(StringUtilities.ToUnixLike(propertyName));
				}
			}

			return modelBuilder;
		}
	}
}