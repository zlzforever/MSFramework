using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public class EventSouringDbContext : DbContextBase
	{
		public EventSouringDbContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory) : base(options, typeFinder, loggerFactory)
		{
		}
	}
}