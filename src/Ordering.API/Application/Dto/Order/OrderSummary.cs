using System;

namespace Ordering.API.Application.Dto.Order
{
	public class OrderSummary
	{
		public int OrderNumber { get; set; }
		public DateTime Date { get; set; }
		public string Status { get; set; }
		public double Total { get; set; }
	}
}