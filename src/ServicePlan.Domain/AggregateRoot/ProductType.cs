using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ProductType : Enumeration
	{
		/// <summary>
		/// 数据产品
		/// </summary>
		public static ProductType Data = new ProductType(1, nameof(Data).ToLowerInvariant());
		
		/// <summary>
		/// 调研
		/// </summary>
		public static ProductType Survey = new ProductType(2, nameof(Survey).ToLowerInvariant());
		
		/// <summary>
		/// 电话
		/// </summary>
		public static ProductType Tel = new ProductType(3, nameof(Tel).ToLowerInvariant());
		
		/// <summary>
		/// 会议
		/// </summary>
		public static ProductType Conference = new ProductType(4, nameof(Conference).ToLowerInvariant());
		
		public ProductType(int id, string name) : base(id, name)
		{
		}
	}
}