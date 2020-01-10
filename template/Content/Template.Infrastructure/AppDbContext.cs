using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Ef;
using MSFramework.EventBus;

namespace Template.Infrastructure
{
	public class AppDbContext : DbContextBase
	{
		public AppDbContext(DbContextOptions options, IEventBus eventBus,
			IEntityConfigurationTypeFinder typeFinder, ILoggerFactory loggerFactory) : base(options, eventBus,
			typeFinder, loggerFactory)
		{
		}
	}
}