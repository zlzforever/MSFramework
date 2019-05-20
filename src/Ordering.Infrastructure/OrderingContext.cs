using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore;

namespace Ordering.Infrastructure
{
	public class OrderingContext : DbContextBase
	{
		public OrderingContext(DbContextOptions options, IMediator mediator, IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory) : base(options, mediator, typeFinder,
			loggerFactory)
		{
		}
	}
}