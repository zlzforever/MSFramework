using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;

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