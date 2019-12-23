using System;
using Microsoft.EntityFrameworkCore;

namespace MSFramework.Ef.SqlServer
{
	public class DbContextOptionsBuilderCreator : IDbContextOptionsBuilderCreator
	{
		public string Type => "SqlServer";

		public DbContextOptionsBuilder Create(Type dbContextType, string connectionString)
		{
			DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
			string entryAssemblyName = dbContextType.Assembly.GetName().Name;
			return optionsBuilder.UseSqlServer(connectionString, builder =>
			{
				builder.MigrationsAssembly(entryAssemblyName);
			});
		}
	}
}