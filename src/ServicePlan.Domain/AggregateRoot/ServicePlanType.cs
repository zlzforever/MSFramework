using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ServicePlanType: Enumeration
	{
		/// <summary>
		/// 数据产品
		/// </summary>
		public static ServicePlanType Data = new ServicePlanType(1, nameof(Data).ToLowerInvariant());
		
		/// <summary>
		/// 调研
		/// </summary>
		public static ServicePlanType Survey = new ServicePlanType(2, nameof(Survey).ToLowerInvariant());
		
		/// <summary>
		/// 电话
		/// </summary>
		public static ServicePlanType Tel = new ServicePlanType(3, nameof(Tel).ToLowerInvariant());
		
		/// <summary>
		/// 会议
		/// </summary>
		public static ServicePlanType Conference = new ServicePlanType(4, nameof(Conference).ToLowerInvariant());
		
		/// <summary>
		/// 路演
		/// </summary>
		public static ServicePlanType RoadShow = new ServicePlanType(5, nameof(RoadShow).ToLowerInvariant());
		
		public ServicePlanType(int id, string name) : base(id, name)
		{
		}
	}
}