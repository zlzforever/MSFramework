using System;

namespace MSFramework.Ef.Function
{
	public class FunctionEfRepository : EfRepository<MSFramework.Function.Function, Guid>
	{
		public FunctionEfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}