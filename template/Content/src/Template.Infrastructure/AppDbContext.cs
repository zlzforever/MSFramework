using System;
using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;

namespace Template.Infrastructure
{
	public class AppDbContext : DbContextBase
	{
		public AppDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
		{
		}
	}
}