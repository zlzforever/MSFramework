namespace MSFramework.Ef.Function
{
	public class FunctionEfRepository : EfRepository<MSFramework.Function.Function>
	{
		public FunctionEfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}