using System;
using System.Reflection;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 服务计划
	/// </summary>
	public class ServicePlan
	{
		/// <summary>
		/// 产品
		/// </summary>
		private Product _product;

		/// <summary>
		/// 计划名称
		/// </summary>
		private string _name;

		/// <summary>
		/// 提交状态
		/// </summary>
		private SubmitState _submitState;

		/// <summary>
		/// 审核状态
		/// </summary>
		private ValidationState _validationState;

		/// <summary>
		/// 开始时间
		/// </summary>
		private DateTime _beginTime;

		/// <summary>
		/// 结束时间
		/// </summary>
		private DateTime _endTime;

		/// <summary>
		/// 负责人， 服务计划的团队以负责人为准
		/// </summary>
		private User _user;

		/// <summary>
		/// 创建人
		/// </summary>
		private User _creator;
		
		/// <summary>
		/// 批次
		/// PS: 一个需求针对多个产品，所以一次生成多个服务计划，有相同的批次
		/// </summary>
		private string _batch;

		public ServicePlan( Product product, string name)
		{
			
		}
	}
}