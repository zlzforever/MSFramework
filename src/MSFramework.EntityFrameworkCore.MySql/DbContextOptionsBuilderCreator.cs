using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MSFramework.EntityFrameworkCore.MySql
{
	public class DbContextOptionsBuilderCreator : IDbContextOptionsBuilderCreator
	{
		public string Type => "SqlServer";

		public DbContextOptionsBuilder Create(Type dbContextType, string connectionString)
		{
			DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
			string entryAssemblyName = dbContextType.Assembly.GetName().Name;
			return optionsBuilder.UseMySql(connectionString,
				builder => { builder.MigrationsAssembly(entryAssemblyName); });
		}
	}
}