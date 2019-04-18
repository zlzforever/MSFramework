using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Domain.Repositories
{
	public interface IQueryRepository<TEntity, TKey>
	{
		/// <summary>
		/// 根据主键获取对象
		/// </summary>
		/// <param name="id">主键值</param>
		/// <returns></returns>
		TEntity Get(TKey id);

		/// <summary>
		/// 根据主键异步获取对象
		/// </summary>
		/// <param name="id">主键值</param>
		/// <param name="cancellationToken">引发取消异常的对象</param>
		/// <returns></returns>
		Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

		List<TEntity> GetList();

		Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

		List<TEntity> GetList(TKey[] ids);

		Task<List<TEntity>> GetListAsync(TKey[] ids, CancellationToken cancellationToken = default);

		long GetCount();

		Task<long> GetCountAsync(CancellationToken cancellationToken = default);
	}
}
