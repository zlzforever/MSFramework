using System;
using MediatR;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.API.Application.Query
{
	public class GetOrderByIdQuery : IRequest<Order>
	{
		public Guid OrderId { get; }

		public GetOrderByIdQuery(Guid id)
		{
			OrderId = id;
		}
	}
}