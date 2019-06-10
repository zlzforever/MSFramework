using System;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace MSFramework.EntityFrameworkCore.MySql
{
	public class DbContextOptionsBuilderCreator : IDbContextOptionsBuilderCreator
	{
		public string Type => "MySql";

		public DbContextOptionsBuilder Create(Type dbContextType, string connectionString)
		{
			DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
			string entryAssemblyName = dbContextType.Assembly.GetName().Name;
			return optionsBuilder.UseMySql(connectionString,
				builder =>
				{
					builder.MigrationsAssembly(entryAssemblyName);
					builder.UnicodeCharSet(CharSet.Utf8mb4);
				});
		}
	}
}