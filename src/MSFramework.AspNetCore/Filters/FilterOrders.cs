namespace MSFramework.AspNetCore.Filters
{
	public static class FilterOrders
	{
		public const int AuditFilterOrder = 3000;
		public const int UnitOfWorkFilterOrder = 4000;
		public const int FunctionFilterOrder = 0;
	}
}