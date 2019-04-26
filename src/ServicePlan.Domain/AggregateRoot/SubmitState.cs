using System;
using System.Collections.Generic;
using System.Text;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class SubmitState : Enumeration
	{
		public static SubmitState AwaitingSubmit = new SubmitState(1, nameof(AwaitingSubmit).ToLowerInvariant());
		public static SubmitState Submitted = new SubmitState(2, nameof(Submitted).ToLowerInvariant());
		
		public SubmitState(int id, string name) : base(id, name)
		{
		}
	}
}
