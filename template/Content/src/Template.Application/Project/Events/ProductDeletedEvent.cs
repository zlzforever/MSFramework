using MicroserviceFramework.EventBus;
using MongoDB.Bson;

namespace Template.Application.Project.Events
{
	public class ProductDeletedEvent : EventBase
	{
		public ObjectId ProductId { get; private set; }

		public ProductDeletedEvent(ObjectId productId)
		{
			ProductId = productId;
		}

		public override string ToString()
		{
			return $"[{GetType().Name}] ProductId = {ProductId}";
		}
	}
}