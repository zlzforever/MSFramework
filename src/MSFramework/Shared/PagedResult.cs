using System.Collections.Generic;

namespace MicroserviceFramework.Shared
{
	public class PagedResult<TEntity>
	{
		public IEnumerable<TEntity> Data { get; private set; }

		/// <summary>
		/// 总计
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// 当前页数 
		/// </summary>
		public int Page { get; private set; }

		/// <summary>
		/// 每页数据量 
		/// </summary>
		public int Limit { get; private set; }

		public PagedResult(int page, int limit, int count, IEnumerable<TEntity> data)
		{
			Page = page;
			Limit = limit;
			Count = count;
			Data = data;
		}
	}
}