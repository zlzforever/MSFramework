using MSFramework.Domain;

namespace Template.Domain.AggregateRoot
{
	public class ProductType : Enumeration
	{
		/// <summary>
		/// 家居
		/// </summary>
		public static ProductType Home = new ProductType(nameof(Home), nameof(Home));

		/// <summary>
		/// 美妆
		/// </summary>
		public static ProductType Beauty = new ProductType(nameof(Beauty), nameof(Beauty));

		public ProductType(string id, string name) : base(id, name)
		{
		}
	}
}