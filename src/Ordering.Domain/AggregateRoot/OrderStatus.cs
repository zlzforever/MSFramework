using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoot
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