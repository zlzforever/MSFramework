using System;
using System.Collections.Generic;
using System.Text;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class ServicePlanState : Enumeration
	{
		public static ServicePlanState AwaitingSubmit = new ServicePlanState(1, nameof(AwaitingSubmit).ToLowerInvariant());
		public static ServicePlanState Submitted = new ServicePlanState(2, nameof(Submitted).ToLowerInvariant());
		public static ServicePlanState AwaitingAudit = new ServicePlanState(3, nameof(AwaitingAudit).ToLowerInvariant());
		public static ServicePlanState AwaitingComplete = new ServicePlanState(4, nameof(AwaitingComplete).ToLowerInvariant());
		public static ServicePlanState Complete = new ServicePlanState(5, nameof(Complete).ToLowerInvariant());
		
		public ServicePlanState(int id, string name) : base(id, name)
		{
		}
	}
}
