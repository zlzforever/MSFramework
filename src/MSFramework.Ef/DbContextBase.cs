using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Audit;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.Infrastructure;
using MicroserviceFramework.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef
{
	public abstract class DbContextBase : DbContext, IUnitOfWork
	{
		private ILogger _logger;
		private readonly ISession _session;
		private readonly DbContextConfigurationCollection _entityFrameworkOptions;
		private IEntityConfigurationTypeFinder _entityConfigurationTypeFinder;
		private readonly IDomainEventDispatcher _domainEventDispatcher;
		private readonly UnitOfWorkManager _unitOfWorkManager;
		private IEventBus _eventBus;

		/// <summary>
		/// 初始化一个<see cref="DbContextBase"/>类型的新实例
		/// </summary>
		protected DbContextBase(DbContextOptions options,
			IOptions<DbContextConfigurationCollection> entityFrameworkOptions,
			UnitOfWorkManager unitOfWorkManager,
			IDomainEventDispatcher domainEventDispatcher,
			ISession session)
			: base(options)
		{
			_unitOfWorkManager = unitOfWorkManager;
			_domainEventDispatcher = domainEventDispatcher;
			_entityFrameworkOptions = entityFrameworkOptions.Value;
			_session = session;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

			var option = _entityFrameworkOptions.Get(GetType());
			Database.AutoTransactionsEnabled = option.AutoTransactionsEnabled;

			if (option.EnableSensitiveDataLogging)
			{
				optionsBuilder.EnableSensitiveDataLogging();
			}

			_unitOfWorkManager.Register(this);
		}

		/// <summary>
		/// 创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
		/// </summary>
		/// <param name="modelBuilder">上下文数据模型构建器</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_logger = this.GetService<ILoggerFactory>().CreateLogger(GetType());
			_entityConfigurationTypeFinder = this.GetService<IEntityConfigurationTypeFinder>();
			_eventBus = this.GetService<IEventBus>();

			//通过实体配置信息将实体注册到当前上下文
			var contextType = GetType();

			var registers = _entityConfigurationTypeFinder.GetEntityRegisters(contextType);
			foreach (var register in registers)
			{
				register.RegisterTo(modelBuilder);
			}

			_logger.LogInformation(
				$"将 {registers.Length} 个实体 {string.Join("、", registers.Select(x => x.EntityType))} 注册到上下文 {contextType} 中");

			modelBuilder.UseObjectId();

			var option = _entityFrameworkOptions.Get(GetType());
			if (option.UseUnderScoreCase)
			{
				modelBuilder.UseUnderScoreCase();
			}
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

		public async Task<int> CommitAsync()
		{
			try
			{
				// 若是有领域事件则分发出去
				// 领域事件可能导致别聚合调用当前 DbContext 并改变状态，或者添加新的事件
				List<DomainEvent> domainEvents;
				// 缓存所有事件，在完成数据库提交后，进行事件的分发
				var events = new List<object>();
				do
				{
					domainEvents = GetDomainEvents();
					foreach (var @event in domainEvents)
					{
						await _domainEventDispatcher.DispatchAsync(@event);
						events.Add(@event);
					}
				} while (domainEvents.Count > 0);

				var effectedCount = 0;
				if (ChangeTracker.Entries().Any())
				{
					ApplyConcepts();
					effectedCount = await SaveChangesAsync();
					if (Database.CurrentTransaction != null)
					{
						await Database.CurrentTransaction.CommitAsync();
					}
				}

				if (_eventBus != null)
				{
					// 集成事件应该在聚合变更完全提交到数据库后才能发布
					foreach (var @event in events)
					{
						await _eventBus.PublishIfEventAsync(@event);
					}
				}

				return effectedCount;
			}
			catch
			{
				if (Database.CurrentTransaction != null)
				{
					await Database.CurrentTransaction.RollbackAsync();
				}

				throw;
			}
		}

		public Guid Id => ContextId.InstanceId;

		protected void ApplyConcepts()
		{
			var userId = _session.UserId;
			var userName = _session.UserName;

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
			if (entry.Entity is ICreation creationAudited)
			{
				creationAudited.SetCreation(userId, userName);
			}
		}

		protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is IModification creationAudited)
			{
				creationAudited.SetModification(userId, userName);
			}
		}

		protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is IDeletion deletionAudited)
			{
				entry.Reload();
				entry.State = EntityState.Modified;

				deletionAudited.Delete(userId, userName);
			}
		}

		private List<DomainEvent> GetDomainEvents()
		{
			// Dispatch Domain Events collection. 
			// Choices:
			// A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
			// side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
			// B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
			// You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 

			var domainEvents = new List<DomainEvent>();

			foreach (var aggregateRoot in ChangeTracker
				.Entries<IAggregateRoot>())
			{
				var events = aggregateRoot.Entity.GetDomainEvents();
				if (events != null && events.Any())
				{
					domainEvents.AddRange(events);
					aggregateRoot.Entity.ClearDomainEvents();
				}
			}

			return domainEvents;
		}
	}
}