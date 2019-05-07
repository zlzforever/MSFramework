using System;
using MSFramework.Domain.Event;

namespace ServicePlan.Domain.AggregateRoot
{
	public class CancelRoadShowServicePlanEvent : DomainEventBase<Guid>
	{
		public Guid AppointmentId { get; }

		public CancelRoadShowServicePlanEvent(Guid appointmentId)
		{
			AppointmentId = appointmentId;
		}
	}
}