using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Shared;

namespace Ordering.Domain.AggregateRoots
{
	public class ProjectCreateEvent : DomainEvent
	{
	}

	public class Product : AggregateRoot, IOptimisticLock
	{
		private Product(ObjectId id) : base(id)
		{
		}

		public static Product Create(string name, int price)
		{
			var product = new Product(ObjectId.NewId())
			{
				Name = name,
				Price = price
			};

			product.AddDomainEvent(new ProjectCreateEvent());
			return product;
		}

		public string Name { get; private set; }

		public int Price { get; private set; }

		public override string ToString()
		{
			return $"[ENTITY {GetType().Name}] Id = {Id}, Name = {Name}, Price = {Price}";
		}

		public string ConcurrencyStamp { get; set; }

		public void SetName(string name)
		{
			Name = name;
		}
	}
}