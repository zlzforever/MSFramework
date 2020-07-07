using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Audit;
using MSFramework.Domain;
using MSFramework.Domain.AggregateRoot;
using MSFramework.Domain.Event;
using MSFramework.Ef.Infrastructure;

namespace MSFramework.Ef
{
	public abstract class DbContextBase : DbContext, IUnitOfWork
	{
		private ILogger _logger;
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// 初始化一个<see cref="DbContextBase"/>类型的新实例
		/// </summary>
		protected DbContextBase(DbContextOptions options, IServiceProvider serviceProvider)
			: base(options)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// 创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
		/// </summary>
		/// <param name="modelBuilder">上下文数据模型构建器</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_logger = _serviceProvider.GetService<ILoggerFactory>().CreateLogger(GetType());

			//通过实体配置信息将实体注册到当前上下文
			var contextType = GetType();
			var registers = _serviceProvider.GetService<IEntityConfigurationTypeFinder>()
				.GetEntityRegisters(contextType);
			foreach (var register in registers)
			{
				register.RegisterTo(modelBuilder);
				_logger.LogDebug($"将实体类 “{register.EntityType}” 注册到上下文 “{contextType}” 中");
			}

			_logger.LogInformation($"上下文 “{contextType}” 注册了 {registers.Length} 个实体类");
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
				var changed = ChangeTracker.Entries().Any();
				if (!changed)
				{
					return 0;
				}

				ApplyConcepts();

				var eventDispatcher = _serviceProvider.GetService<IEventDispatcher>();
				if (eventDispatcher != null)
				{
					// todo: 同步异步混用是否会死锁
					HandleDomainEventsAsync().GetAwaiter().GetResult();
				}

				var effectCount = base.SaveChanges();
				Database.CurrentTransaction?.Commit();
				return effectCount;
			}
			catch
			{
				Database.CurrentTransaction?.Rollback();
				throw;
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
				var changed = ChangeTracker.Entries().Any();
				if (!changed)
				{
					return 0;
				}

				ApplyConcepts();

				await HandleDomainEventsAsync();

				var effectedCount = await base.SaveChangesAsync(cancellationToken);
				if (Database.CurrentTransaction != null)
				{
					await Database.CurrentTransaction.CommitAsync(cancellationToken);
				}

				return effectedCount;
			}
			catch
			{
				if (Database.CurrentTransaction != null)
				{
					await Database.CurrentTransaction.RollbackAsync(cancellationToken);
				}

				throw;
			}
		}

		private async Task HandleDomainEventsAsync()
		{
			var eventDispatcher = _serviceProvider.GetService<IEventDispatcher>();
			if (eventDispatcher == null)
			{
				return;
			}

			var domainEvents = GetDomainEvents();

			foreach (var @event in domainEvents)
			{
				await eventDispatcher.DispatchAsync(@event);
			}
		}

		private IEnumerable<IEvent> GetDomainEvents()
		{
			// Dispatch Domain Events collection. 
			// Choices:
			// A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
			// side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
			// B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
			// You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 

			var domainEvents = new List<IEvent>();

			foreach (var aggregateRoot in ChangeTracker
				.Entries<IAggregateRoot>())
			{
				var events = aggregateRoot.Entity.GetDomainEvents();
				if (events != null && events.Any())
				{
					domainEvents.AddRange(events);
					domainEvents.Clear();
				}
			}

			return domainEvents;
		}

		public IEnumerable<AuditEntity> GetAuditEntities()
		{
			var entities = new List<AuditEntity>();
			foreach (var entry in ChangeTracker.Entries())
			{
				AuditEntity auditEntity = null;
				switch (entry.State)
				{
					case EntityState.Added:
						auditEntity = GetAuditEntity(entry, OperationType.Add);
						break;
					case EntityState.Modified:
						auditEntity = GetAuditEntity(entry, OperationType.Modify);
						break;
					case EntityState.Deleted:
						auditEntity = GetAuditEntity(entry, OperationType.Delete);
						break;
				}

				if (auditEntity != null)
				{
					entities.Add(auditEntity);
				}
			}

			return entities;
		}

		public void Commit()
		{
			SaveChanges();
		}

		public async Task CommitAsync()
		{
			await SaveChangesAsync();
		}

		protected void ApplyConcepts()
		{
			var session = _serviceProvider.GetService<ISession>();

			var userId = session?.UserId;
			var userName = session?.UserName;

			foreach (var entry in ChangeTracker.Entries())
			{
				switch (entry.State)
				{
					case EntityState.Added:
						ApplyConceptsForAddedEntity(entry, userId, userName);
						break;
					case EntityState.Modified:
						ApplyConceptsForModifiedEntity(entry, userId, userName);
						break;
					case EntityState.Deleted:
						ApplyConceptsForDeletedEntity(entry, userId, userName);
						break;
				}
			}
		}

		protected virtual AuditEntity GetAuditEntity(EntityEntry entry, OperationType operationType)
		{
			var type = entry.Entity.GetType();
			var typeName = type.FullName;

			string entityId = null;
			var properties = new List<AuditProperty>();
			foreach (var property in entry.CurrentValues.Properties)
			{
				if (property.IsConcurrencyToken)
				{
					continue;
				}

				var propertyName = property.Name;
				var propertyEntry = entry.Property(property.Name);
				if (property.IsPrimaryKey())
				{
					entityId = entry.State == EntityState.Deleted
						? propertyEntry.OriginalValue?.ToString()
						: propertyEntry.CurrentValue?.ToString();
				}

				string propertyType = property.ClrType.ToString();
				string originalValue = null;
				string newValue = null;

				if (entry.State == EntityState.Added)
				{
					newValue = propertyEntry.CurrentValue?.ToString();
				}
				else if (entry.State == EntityState.Deleted)
				{
					originalValue = propertyEntry.OriginalValue?.ToString();
				}
				else if (entry.State == EntityState.Modified)
				{
					var currentValue = propertyEntry.CurrentValue?.ToString();
					originalValue = propertyEntry.OriginalValue?.ToString();
					if (currentValue == originalValue)
					{
						continue;
					}

					newValue = currentValue;
				}

				if (string.IsNullOrWhiteSpace(originalValue))
				{
					// 原值为空，新值不为空则记录
					if (!string.IsNullOrWhiteSpace(newValue))
					{
						properties.Add(new AuditProperty(propertyName, propertyType, originalValue, newValue));
					}
				}
				else
				{
					if (!originalValue.Equals(newValue))
					{
						properties.Add(new AuditProperty(propertyName, propertyType, originalValue, newValue));
					}
				}
			}

			var auditedEntity = new AuditEntity(typeName, entityId, operationType);
			auditedEntity.AddProperties(properties);
			return auditedEntity;
		}

		protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is ICreationAudited creationAudited)
			{
				creationAudited.SetCreationAudited(userId, userName);
			}
		}

		protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is IModificationAudited creationAudited)
			{
				creationAudited.SetModificationAudited(userId, userName);
			}
		}

		protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is IDeletionAudited deletionAudited)
			{
				entry.Reload();
				entry.State = EntityState.Modified;

				deletionAudited.Delete(userId, userName);
			}
		}
	}
}