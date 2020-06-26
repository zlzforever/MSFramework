using System;
using MSFramework.Common;
using MSFramework.Domain.Event;

namespace Template.Domain.Event
{
	public class PriceOfProductChangedEvent : EventBase
	{
		public Guid ProductId { get; private set; }

		public string ProductName { get; private set; }

		public int OriginPrice { get; private set; }

		public int NewPrice { get; private set; }

		public PriceOfProductChangedEvent(Guid productId,
			string productName,
			int originPrice,
			int newPrice, object source) : base(CombGuid.NewGuid(), DateTimeOffset.Now, source)
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