using MSFramework.Domain.Event;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IAggregateRoot<TEntity, TKey>
	{
		TEntity Insert(TEntity entity, bool autoSave = false);

		Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

		TEntity Update(TEntity entity, bool autoSave = false);

		Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

		void Delete(TEntity entity, bool autoSave = false);

		void Delete(TKey id, bool autoSave = false);

		Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

		Task DeleteAsync(TKey id, bool autoSave = false, CancellationToken cancellationToken = default);
	}
}
