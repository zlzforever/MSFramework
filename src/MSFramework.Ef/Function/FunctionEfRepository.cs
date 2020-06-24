using System;

namespace MSFramework.Ef.Function
{
	public class FunctionEfRepository : EfRepository<MSFramework.Function.FunctionDefine, Guid>
	{
		public FunctionEfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}