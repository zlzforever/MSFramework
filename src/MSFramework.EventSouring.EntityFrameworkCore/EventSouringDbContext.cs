using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public class EventSouringDbContext : DbContextBase
	{
		public EventSouringDbContext(DbContextOptions options, IMediator mediator,
			IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory) : base(options, mediator, typeFinder, loggerFactory)
		{
		}
	}
}