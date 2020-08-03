using MSFramework.Common;
using MSFramework.Domain;

namespace Ordering.Domain.AggregateRoots
{
	public class Product : AggregateRoot
	{
		public Product(string name, int price) : base(ObjectId.NewId())
		{
			Name = name;
			Price = price;
		}

		public string Name { get; private set; }

		public int Price { get; private set; }

		public override string ToString()
		{
			return $"[ENTITY {GetType().Name}] Id = {Id}, Name = {Name}, Price = {Price}";
		}
	}
}