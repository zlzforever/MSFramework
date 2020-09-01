using System;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options,
			serviceProvider)
		{
		}
	}
}