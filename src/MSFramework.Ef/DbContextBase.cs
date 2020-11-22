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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef
{
	public abstract class DbContextBase : DbContext, IUnitOfWork
	{
		private readonly ILogger _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly UnitOfWorkManager _unitOfWorkManager;

		/// <summary>
		/// 初始化一个<see cref="DbContextBase"/>类型的新实例
		/// </summary>
		protected DbContextBase(DbContextOptions options,
			UnitOfWorkManager unitOfWorkManager, IServiceProvider serviceProvider)
			: base(options)
		{
			_serviceProvider = serviceProvider;
			_logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(GetType());
			_unitOfWorkManager = unitOfWorkManager;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

			var option = _serviceProvider
				.GetRequiredService<EntityFrameworkOptionsConfiguration>().Get(GetType());
			Database.AutoTransactionsEnabled = option.AutoTransactionsEnabled;

			if (option.EnableSensitiveDataLogging)
			{
				optionsBuilder.EnableSensitiveDataLogging();
			}

			if (option.LazyLoadingProxiesEnabled)
			{
				optionsBuilder.UseLazyLoadingProxies();
			}

			_unitOfWorkManager.Register(this);
		}

		/// <summary>
		/// 创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
		/// </summary>
		/// <param name="modelBuilder">上下文数据模型构建器</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//通过实体配置信息将实体注册到当前上下文
			var contextType = GetType();
			var registers = _serviceProvider.GetRequiredService<IEntityConfigurationTypeFinder>()
				.GetEntityRegisters(contextType);
			foreach (var register in registers)
			{
				register.RegisterTo(modelBuilder);
				_logger.LogDebug($"将实体类 “{register.EntityType}” 注册到上下文 “{contextType}” 中");
			}

			modelBuilder.UseObjectId();

			var option = _serviceProvider.GetRequiredService<EntityFrameworkOptionsConfiguration>().Get(GetType());
			if (option.UseUnixLikeName)
			{
				modelBuilder.UseUnderScoreCase();
			}

			_logger.LogInformation($"上下文 “{contextType}” 注册了 {registers.Length} 个实体类");
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
				var changed = ChangeTracker.Entries().Any();
				if (!changed)
				{
					return 0;
				}

				ApplyConcepts();

				var dispatcher = _serviceProvider.GetService<IDomainEventDispatcher>();
				if (dispatcher != null)
				{
					var domainEvents = GetDomainEvents();

					foreach (var @event in domainEvents)
					{
						await dispatcher.DispatchAsync(@event);
					}
				}

				var effectedCount = await SaveChangesAsync();
				if (Database.CurrentTransaction != null)
				{
					await Database.CurrentTransaction.CommitAsync();
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

		protected void ApplyConcepts()
		{
			var session = _serviceProvider.GetService<ISession>();

			string userId = null;
			string userName = null;

			if (session != null)
			{
				userId = session.UserId;
				userName = session.UserName;
			}

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

		private IEnumerable<DomainEvent> GetDomainEvents()
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