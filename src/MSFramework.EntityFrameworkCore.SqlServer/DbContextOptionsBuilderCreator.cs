using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MSFramework.EntityFrameworkCore.SqlServer
{
	public class DbContextOptionsBuilderCreator : IDbContextOptionsBuilderCreator
	{
		public string Type => "SqlServer";

		public DbContextOptionsBuilder Create(string connectionString)
		{
			DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
			string entryAssemblyName = Assembly.GetEntryAssembly().GetName().Name;
			return optionsBuilder.UseSqlServer(connectionString, builder =>
			{
				builder.UseRowNumberForPaging();
				builder.MigrationsAssembly(entryAssemblyName);
			});
		}
	}
}