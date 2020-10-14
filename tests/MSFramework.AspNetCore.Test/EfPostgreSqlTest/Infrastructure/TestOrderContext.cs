using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

namespace MSFramework.AspNetCore.Test.EfPostgreSqlTest.Infrastructure
{
	public class TestDataContext : DbContextBase
	{
		public TestDataContext(DbContextOptions options, UnitOfWorkManager unitOfWorkManager, IServiceProvider serviceProvider) : base(options, unitOfWorkManager, serviceProvider)
		{
		}
	}
}