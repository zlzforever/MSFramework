using MSFramework.Domain;

namespace Template.Domain.AggregateRoot
{
	public class ProductType : Enumeration
	{
		/// <summary>
		/// 家居
		/// </summary>
		public static ProductType Home = new ProductType(1, nameof(Home));

		/// <summary>
		/// 美妆
		/// </summary>
		public static ProductType Beauty = new ProductType(2, nameof(Home));

		public ProductType(int id, string name) : base(id, name)
		{
		}
	}
}