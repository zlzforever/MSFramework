using System;
using System.Collections.Generic;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ServiceRecord : EntityBase<Guid>
	{
		/// <summary>
		/// 服务时间
		/// </summary>
		private DateTime _serviceTime;

		/// <summary>
		/// 服务计划类型
		/// </summary>
		private ServicePlanType _servicePlanType;
		
		/// <summary>
		/// 主题
		/// </summary>
		private string _subject;

		/// <summary>
		/// 行业
		/// </summary>
		private string _industryId;

		/// <summary>
		/// 客户联系人
		/// </summary>
		private readonly ClientUser _clientUser;

		/// <summary>
		/// 客户关注点及原因
		/// </summary>
		private string _clientFocusKeyPoint;

		/// <summary>
		/// 是否继续委托
		/// </summary>
		private bool _continue;

		/// <summary>
		/// 修改要求
		/// </summary>
		private string _modificationRequirement;

		/// <summary>
		/// 新增需求
		/// </summary>
		private string _newRequirement;

		/// <summary>
		/// 销售打分
		/// </summary>
		private string _scoring;

		/// <summary>
		/// 销售反馈
		/// </summary>
		private string _feedback;
		
		public ServiceRecord(DateTime serviceTime, ServicePlanType planType, string subject, string industryId, ClientUser clientUser)
		{
			_serviceTime = serviceTime;
			_servicePlanType = planType;
			_subject = subject;
			_industryId = industryId;
			_clientUser = clientUser;
		}
	}
}