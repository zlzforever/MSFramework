using System;
using MediatR;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IDomainEvent : IEvent
	{
	}
}