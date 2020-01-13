using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Ef;
using MSFramework.EventBus;

namespace MSFramework.Permission.Ef
{
	public class PermissionDbContext : DbContextBase
	{
		public PermissionDbContext(DbContextOptions options)
			: base(options)
		{
		}
	}
}