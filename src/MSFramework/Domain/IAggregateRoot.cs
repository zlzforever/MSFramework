using System;
using System.Collections.Generic;
using MSFramework.Domain.Event;

namespace MSFramework.Domain
{
	/// <summary>
	/// Represents an aggregate root.
	/// </summary>
	public interface IAggregateRoot
	{
		/// <summary>
		/// the id as string of the aggregate root.
		/// </summary>
		/// <returns></returns>
		string GetId();
		
		int Version { get; }		

		IEnumerable<IDomainEvent> GetChanges();
		
		void ClearChanges();	
		
		void LoadFromHistory(params IDomainEvent[] histories);
	}
}