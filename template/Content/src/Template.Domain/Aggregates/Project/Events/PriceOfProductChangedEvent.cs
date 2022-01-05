using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Template.Domain.Aggregates.Project.Events
{
	public class PriceOfProductChangedEvent : DomainEvent
	{
		public ObjectId ProductId { get; private set; }

		public string ProductName { get; private set; }

		public int OriginPrice { get; private set; }

		public int NewPrice { get; private set; }

		public PriceOfProductChangedEvent(ObjectId productId,
			string productName,
			int originPrice,
			int newPrice)
		{
			ProductId = productId;
			ProductName = productName;
			OriginPrice = originPrice;
			NewPrice = newPrice;
		}

		public override string ToString()
		{
			return
				$"[{GetType().Name}] ProductId = {ProductId}, ProductName = {ProductName}, OriginPrice = {OriginPrice}, NewPrice = {NewPrice}";
		}
	}
}