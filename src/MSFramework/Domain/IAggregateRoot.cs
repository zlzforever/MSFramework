using System.Collections.Generic;
using MediatR;

namespace MSFramework.Domain
{
	public interface IAggregateRoot
	{
		IEnumerable<INotification> GetDomainEvents();

		void ClearDomainEvents();
	}
}