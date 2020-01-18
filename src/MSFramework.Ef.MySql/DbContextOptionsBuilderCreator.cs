using System;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace MSFramework.Ef.MySql
{
	public class DbContextOptionsBuilderCreator : IDbContextOptionsBuilderCreator
	{
		public string Type => "MySql";

		public DbContextOptionsBuilder Create(Type dbContextType,
			string connectionString)
		{
			var optionsBuilder = new DbContextOptionsBuilder();
			var entryAssemblyName = dbContextType.Assembly.GetName().Name;
			return optionsBuilder.UseMySql(connectionString,
				builder =>
				{
					builder.MigrationsAssembly(entryAssemblyName);
					builder.CharSet(CharSet.Utf8Mb4);
				});
		}
	}
}