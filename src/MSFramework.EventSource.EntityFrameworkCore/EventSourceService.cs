using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.EntityFrameworkCore.Repository;
using MSFramework.EventBus;

namespace MSFramework.EventSource.EntityFrameworkCore
{
	public class EventSourceService : IEventSourceService
	{
		private readonly IEfRepository<EventEntry, int> _repository;
		private readonly ILogger _logger;
		private readonly IEventBus _eventBus;

		public EventSourceService(IEventBus eventBus,
			IEfRepository<EventEntry, int> repository, ILogger<EventSourceService> logger)
		{
			_repository = repository;
			_eventBus = eventBus;
			_logger = logger;
		}

		public async Task PublishEventsAsync()
		{
			var pendindLogEvents = await GetAllPendingEventListAsync();

			foreach (var logEvt in pendindLogEvents)
			{
				var @event = logEvt.ToEvent();
				_logger.LogInformation(
					"----- Publishing integration event: {IntegrationEventId} from {AppName}", logEvt.Id,
					Assembly.GetEntryAssembly().GetName().Name);

				try
				{
					await ProgressEventAsync(logEvt.EventId);
					await _eventBus.PublishAsync(@event);
					await PublishedEventAsync(logEvt.EventId);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId} from {AppName}",
						logEvt.EventId, Assembly.GetEntryAssembly().GetName().Name);

					await FailedEventAsync(logEvt.EventId);
				}
			}
		}

		public async Task AddEventAsync(Event @event)
		{
			var entry = new EventEntry(@event);
			await _repository.InsertAsync(entry);
			await _repository.Context.SaveChangesAsync();
		}

		private async Task<List<EventEntry>> GetAllPendingEventListAsync()
		{
			return await _repository
				.GetAll().Where(e => e.Status == EventStatus.Ready)
				.OrderBy(o => o.CreationTime)
				.ToListAsync();
		}
		
		public async Task PublishedEventAsync(Guid eventId)
		{
			await UpdateEventStatus(eventId, EventStatus.Published);
		}

		public async Task ProgressEventAsync(Guid eventId)
		{
			await UpdateEventStatus(eventId, EventStatus.InProgress);
		}

		public async Task FailedEventAsync(Guid eventId)
		{
			await UpdateEventStatus(eventId, EventStatus.PublishedFailed);
		}

		private async Task UpdateEventStatus(Guid eventId, EventStatus status)
		{
			var entry = await _repository.Table.FirstOrDefaultAsync(x => x.EventId == eventId);
			entry.Status = status;

			if (status == EventStatus.InProgress)
				entry.SentTimes++;

			await _repository.UpdateAsync(entry);
			await _repository.Context.SaveChangesAsync();
		}
	}
}