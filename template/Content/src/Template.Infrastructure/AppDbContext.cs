using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using MSFramework.Ef;
using MSFramework.Ef.MySql;

namespace Template.Infrastructure
{
	public class AppDbContext : DbContextBase
	{
		public AppDbContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			base.OnConfiguring(builder);
			if (Assembly.GetEntryAssembly()?.GetName().Name == "ef")
			{
				builder.ReplaceService<IProviderConventionSetBuilder, MySqlConventionSetBuilder>();
			}
		}
	}
}