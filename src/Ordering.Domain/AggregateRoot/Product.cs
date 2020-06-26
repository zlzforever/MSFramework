using MSFramework.Common;
using MSFramework.Domain.AggregateRoot;

namespace Ordering.Domain.AggregateRoot
{
	public class Product : AggregateRootBase
	{
		public Product(string name, int price) : base(CombGuid.NewGuid())
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