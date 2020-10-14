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
		public Product(string name, int price) : base(ObjectId.NewId())
		{
			Name = name;
			Price = price;
			
			AddDomainEvent(new ProjectCreateEvent());
		}

		public string Name { get; private set; }

		public int Price { get; private set; }

		public override string ToString()
		{
			return $"[ENTITY {GetType().Name}] Id = {Id}, Name = {Name}, Price = {Price}";
		}

		public string ConcurrencyStamp { get; set; }
	}
}