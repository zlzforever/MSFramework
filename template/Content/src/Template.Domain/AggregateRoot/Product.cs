using MSFramework.Common;
using MSFramework.Domain.AggregateRoot;
using Template.Domain.Event;

namespace Template.Domain.AggregateRoot
{
	public class Product : ModificationAuditedAggregateRoot
	{
		private Product() : base(CombGuid.NewGuid())
		{
		}

		public Product(string name, int price, ProductType productType) : this()
		{
			Name = name;
			Price = price;
			ProductType = productType;
		}

		public string Name { get; private set; }

		public int Price { get; private set; }

		public ProductType ProductType { get; private set; }

		public override string ToString()
		{
			return $"[ENTITY {GetType().Name}] Id = {Id}, Name = {Name}, Price = {Price}, ProductType = {ProductType}";
		}

		public void ChangePrice(int price)
		{
			if (Price != price)
			{
				var @event = new PriceOfProductChangedEvent(Id, Name, Price, price, this);
				Price = price;
				AddDomainEvent(@event);
			}
		}

		public void ChangeName(string name)
		{
			if (!Name.Equals(name))
			{
				Name = name;
			}
		}
	}
}