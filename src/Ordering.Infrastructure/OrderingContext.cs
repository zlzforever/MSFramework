using System;
using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;

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