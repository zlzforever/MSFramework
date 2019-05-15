using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRootBase :
		EntityBase<Guid>,
		IAggregateRoot
	{
		private const int NewAggregateVersion = -1;

		private readonly List<Event> _changes =
			new List<Event>();

		private int _version = NewAggregateVersion;

		protected AggregateRootBase() : base(Singleton<IIdGenerator>.Instance.GetNewId<Guid>())
		{
		}

		protected AggregateRootBase(Guid id) : base(id)
		{
		}

		public int Version
		{
			get => _version;
			protected set => _version = value;
		}

		protected void ApplyChangedEvent(Event @event)
		{
			@event.NotNull(nameof(@event));
			if (Equals(Id, default(Guid)))
			{
				throw new System.Exception("Aggregate root id cannot be null.");
			}

			_version++;
			@event.Id = Id;
			@event.Version = _version;

			this.ToDynamic().Apply(@event);
			_changes.Add(@event);
		}

		protected void ApplyChangedEvents(
			IEnumerable<Event> events)
		{
			if (events == null)
			{
				throw new ArgumentNullException(nameof(events));
			}

			foreach (var @event in events)
			{
				ApplyChangedEvent(@event);
			}
		}

		public IReadOnlyCollection<Event> GetUncommittedChanges() => _changes.AsReadOnly();

		public void ClearChanges() => _changes.Clear();

		public void LoadFromHistory(params Event[] histories)
		{
			foreach (var @event in histories)
			{
				if (@event.Version != _version + 1)
				{
					throw new MSFrameworkException(@event.Id.ToString());
				}

				_id = (Guid) Convert.ChangeType(@event.Id, typeof(Guid));
				_version = @event.Version;
				this.ToDynamic().Apply(@event);
			}
		}
	}
}