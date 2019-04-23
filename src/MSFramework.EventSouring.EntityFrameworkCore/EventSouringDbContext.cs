using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore;
using MSFramework.EventBus;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public class EventSouringDbContext : DbContextBase
	{
		public EventSouringDbContext(DbContextOptions options, IEntityConfigurationTypeFinder typeFinder,
			IEventBus eventBus, ILoggerFactory loggerFactory) : base(options, typeFinder, eventBus, loggerFactory)
		{
		}
	}
}