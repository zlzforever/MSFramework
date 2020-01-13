using Microsoft.EntityFrameworkCore;
using MSFramework.Ef;

namespace Template.Infrastructure
{
	public class AppDbContext : DbContextBase
	{
		public AppDbContext(DbContextOptions options) : base(options)
		{
		}
	}
}