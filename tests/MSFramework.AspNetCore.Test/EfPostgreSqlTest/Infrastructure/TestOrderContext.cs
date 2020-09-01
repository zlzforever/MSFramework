using System;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

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