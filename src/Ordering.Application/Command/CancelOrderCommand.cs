using System;
using MediatR;
using MSFramework.Http;

namespace Ordering.Application.Command
{
	public class CancelOrderCommand : IRequest<ApiResult>
	{
		public Guid OrderId { get; private set; }

		public CancelOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}