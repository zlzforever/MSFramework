namespace Ordering.Domain.AggregateRoots
{
	public enum OrderStatus
	{
		Submitted,
		AwaitingValidation,
		StockConfirmed,
		Paid,
		Shipped,
		Cancelled
	}
}