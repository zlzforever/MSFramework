using System.Collections.Generic;
using MediatR;

namespace MSFramework.Domain
{
	public interface IEventProvider
	{
		IReadOnlyCollection<INotification> DomainEvents { get; }

		void AddDomainEvent(INotification @event);

		void ClearDomainEvents();
	}
}