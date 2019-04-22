using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using MSFramework.CQRS.EventSouring;
using MSFramework.Domain;
using MSFramework.Domain.Auditing;
using MSFramework.Domain.Entity;
using MSFramework.EventBus;
using MSFramework.Security;

namespace MSFramework.EntityFrameworkCore
{
	public abstract class DbContextBase : DbContext
	{
		private readonly ILogger _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IEntityConfigurationTypeFinder _typeFinder;
		private readonly IEventBus _eventBusr;
		private readonly IEventStore _eventStore;
		private readonly IDistributedEventBus _distributedEventBus;

		/// <summary>
		/// 初始化一个<see cref="DbContextBase"/>类型的新实例
		/// </summary>
		protected DbContextBase(
			DbContextOptions options,
			IEntityConfigurationTypeFinder typeFinder,
			ILocalEventBus mediator,
			IDistributedEventBus distributedEventBus,
			IEventStore eventStore,
			ILoggerFactory loggerFactory)
			: base(options)
		{
			_typeFinder = typeFinder;
			_loggerFactory = loggerFactory;
			_logger = loggerFactory?.CreateLogger(GetType());
			_eventBusr = mediator;
			_eventStore = eventStore;
			_distributedEventBus = distributedEventBus;
		}

		/// <summary>
		/// 创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
		/// </summary>
		/// <param name="modelBuilder">上下文数据模型构建器</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//通过实体配置信息将实体注册到当前上下文
			Type contextType = GetType();
			IEntityRegister[] registers = _typeFinder.GetEntityRegisters(contextType);
			foreach (IEntityRegister register in registers)
			{
				register.RegisterTo(modelBuilder);
				_logger?.LogDebug($"将实体类“{register.EntityType}”注册到上下文“{contextType}”中");
			}

			_logger?.LogInformation($"上下文“{contextType}”注册了{registers.Length}个实体类");
		}

		/// <summary>
		/// 模型配置
		/// </summary>
		/// <param name="optionsBuilder"></param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var options = EntityFrameworkOptions.EntityFrameworkOptionDict.Values.SingleOrDefault(m =>
				m.DbContextType == GetType());
			if (options != null && options.LazyLoadingProxiesEnabled)
			{
				optionsBuilder.UseLazyLoadingProxies();
			}

			if (_loggerFactory != null)
			{
				optionsBuilder.UseLoggerFactory(_loggerFactory);
			}

			optionsBuilder.EnableSensitiveDataLogging();
		}

		/// <summary>
		///     将在此上下文中所做的所有更改保存到数据库中，同时自动开启事务或使用现有同连接事务
		/// </summary>
		/// <remarks>
		///     此方法将自动调用 <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> 
		///     若要在保存到基础数据库之前发现对实体实例的任何更改，请执行以下操作。这可以通过以下类型禁用
		///     <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		/// </remarks>
		/// <returns>
		///     写入数据库的状态项的数目。
		/// </returns>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateException">
		///     保存到数据库时遇到错误。
		/// </exception>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException">
		///     保存到数据库时会遇到并发冲突。
		///     当在保存期间影响到意外数量的行时，就会发生并发冲突。
		///     这通常是因为数据库中的数据在加载到内存后已经被修改。
		/// </exception>
		public override int SaveChanges()
		{
			try
			{
				ApplyConcepts();


				return base.SaveChanges();
			}
			catch (DbUpdateConcurrencyException ex)
			{
				throw new MSFrameworkException(ex.Message, ex);
			}
		}

		protected string UserId => this.GetService<ICurrentUser>().UserId;

		protected virtual void ApplyConcepts()
		{
			foreach (var entry in ChangeTracker.Entries())
			{
				ApplyConcepts(entry, UserId);
			}
		}

		protected virtual void SetConcurrencyStampIfNull(EntityEntry entry)
		{
			var entity = entry.Entity as IHasConcurrencyStamp;
			if (entity == null)
			{
				return;
			}

			if (entity.ConcurrencyStamp != null)
			{
				return;
			}

			entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
		}

		protected virtual void ApplyConcepts(EntityEntry entry, string userId)
		{
			switch (entry.State)
			{
				case EntityState.Added:
					ApplyConceptsForAddedEntity(entry, userId);
					break;
				case EntityState.Modified:
					ApplyConceptsForModifiedEntity(entry, userId);
					break;
				case EntityState.Deleted:
					ApplyConceptsForDeletedEntity(entry, userId);
					break;
			}
		}

		protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry, string userId)
		{
			CheckAndSetId(entry);
			SetConcurrencyStampIfNull(entry);
			EntityAuditingHelper.SetCreationAuditProperties(entry.Entity, userId);
		}

		protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, string userId)
		{
			EntityAuditingHelper.SetModificationAuditProperties(entry.Entity, userId);
			if (entry.Entity is ISoftDelete e && e.IsDeleted)
			{
				SetDeletionAuditProperties(entry.Entity, userId);
			}
		}

		protected virtual void SetDeletionAuditProperties(object entityAsObj, string userId)
		{
			if (entityAsObj is IHasDeletionTime entity1)
			{
				if (entity1.DeletionTime == null)
				{
					entity1.DeletionTime = DateTime.Now;
				}
			}

			if (entityAsObj is IDeletionAudited e)
			{
				if (e.DeleterUserId != null)
				{
					return;
				}

				e.DeleterUserId = userId;
			}
		}

		protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, string userId)
		{
			// todo
			// if (IsHardDeleteEntity(entry))
			// {
			//    return;
			//}

			CancelDeletionForSoftDelete(entry);
			SetDeletionAuditProperties(entry.Entity, userId);
		}

		protected virtual void CancelDeletionForSoftDelete(EntityEntry entry)
		{
			if (entry.Entity is ISoftDelete e)
			{
				entry.Reload();
				entry.State = EntityState.Modified;
				e.IsDeleted = true;
			}
		}

		protected virtual void CheckAndSetId(EntityEntry entry)
		{
			//Set GUID Ids
			if (entry.Entity is IEntity<Guid> entity && entity.Id == Guid.Empty)
			{
				var idPropertyEntry = entry.Property("Id");

				if (idPropertyEntry != null && idPropertyEntry.Metadata.ValueGenerated == ValueGenerated.Never)
				{
					entity.Id = Guid.NewGuid();
				}
			}
		}

		/// <summary>
		///     异步地将此上下文中的所有更改保存到数据库中，同时自动开启事务或使用现有同连接事务
		/// </summary>
		/// <remarks>
		///     <para>
		///         此方法将自动调用 <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> 
		///         若要在保存到基础数据库之前发现对实体实例的任何更改，请执行以下操作。这可以通过以下类型禁用
		///         <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		///     </para>
		///     <para>
		///         不支持同一上下文实例上的多个活动操作。请使用“等待”确保在此上下文上调用其他方法之前任何异步操作都已完成。
		///     </para>
		/// </remarks>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     表示异步保存操作的任务。任务结果包含写入数据库的状态条目数。
		/// </returns>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateException">
		///     保存到数据库时遇到错误。
		/// </exception>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException">
		///     保存到数据库时会遇到并发冲突。
		///     当在保存期间影响到意外数量的行时，就会发生并发冲突。
		///     这通常是因为数据库中的数据在加载到内存后已经被修改。
		/// </exception>
		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			try
			{
				ApplyConcepts();

				// Dispatch Domain Events collection. 
				// Choices:
				// A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
				// side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
				// B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
				// You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
				await DispatchDomainEventsAsync();

				await SaveEventSourceAsync();

				// After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
				// performed through the DbContext will be committed

				return await base.SaveChangesAsync(cancellationToken);
			}
			catch (DbUpdateConcurrencyException ex)
			{
				throw new MSFrameworkException(ex.Message, ex);
			}
		}

		private async Task SaveEventSourceAsync()
		{
			var aggregateRoots = ChangeTracker
				.Entries<IEventSourcingAggregate>()
				.Where(x => x.Entity.GetAggregateEvents() != null && x.Entity.GetAggregateEvents().Any()).ToList();

			foreach (var aggregateRoot in aggregateRoots)
			{
				foreach (var aggregateEvent in aggregateRoot.Entity.GetAggregateEvents())
				{
					await _eventStore.AddEventAsync(new EventSourceEntry(aggregateEvent));
				}
			}
		}

		protected virtual async Task DispatchDomainEventsAsync()
		{
			var aggregateRoots = ChangeTracker
				.Entries<IAggregateRoot>()
				.Where(x => x.Entity.GetLocalDomainEvents() != null && x.Entity.GetLocalDomainEvents().Any()).ToList();

			var localDomainEvents = new List<IDomainEvent>();
			var distributedDomainEvents = new List<IDomainEvent>();
			foreach (var aggregateRoot in aggregateRoots)
			{
				localDomainEvents.AddRange(aggregateRoot.Entity.GetLocalDomainEvents());
				aggregateRoot.Entity.ClearLocalDomainEvents();
				distributedDomainEvents.AddRange(aggregateRoot.Entity.GetDistributedDomainEvents());
				aggregateRoot.Entity.ClearDistributedDomainEvents();
			}

			var tasks = new List<Task>();
			tasks.AddRange(localDomainEvents.Select(async @event =>
			{
				// 通过 EventBus 发布出去
				await _eventBusr.PublishAsync(@event);
			}));
			tasks.AddRange(distributedDomainEvents.Select(async @event =>
			{
				// 通过 Distributed EventBus 发布出去
				await _distributedEventBus.PublishAsync(@event);
			}));
			await Task.WhenAll(tasks);
		}
	}
}