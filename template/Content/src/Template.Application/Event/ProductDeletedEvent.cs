using System;
using MSFramework.Domain.Event;

namespace Template.Application.Event
{
	public class ProductDeletedEvent : IntegrationEvent
	{
		public Guid ProductId { get; private set; }

		public ProductDeletedEvent(Guid productId)
		{
			ProductId = productId;
		}

		public override string ToString()
		{
			return $"[{GetType().Name}] ProductId = {ProductId}";
		}
	}
}