using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using MSFramework.Ef;
using MSFramework.Ef.MySql;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			if (Assembly.GetEntryAssembly()?.GetName().Name == "ef")
			{
				optionsBuilder.ReplaceService<IProviderConventionSetBuilder, MySqlConventionSetBuilder>();
			}
		}
	}
}