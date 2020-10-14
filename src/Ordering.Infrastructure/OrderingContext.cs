using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, 
			UnitOfWorkManager unitOfWorkManager, 
			IServiceProvider serviceProvider) : base(options, unitOfWorkManager, serviceProvider)
		{
		}
	}
}