using System;
using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure
{
	public class TestDataContext : DbContextBase
	{
		public TestDataContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options,
			serviceProvider)
		{
		}
	}
}