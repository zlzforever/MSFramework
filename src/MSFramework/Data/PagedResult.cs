using System.Collections.Generic;
using System.Linq;

namespace MSFramework.Data
{
	public abstract class PagedResult
	{
		/// <summary>
		/// 总计
		/// </summary>
		public int Count { get; protected set; }

		/// <summary>
		/// 当前页数 
		/// </summary>
		public int Page { get; protected set; }

		/// <summary>
		/// 每页数据量 
		/// </summary>
		public int Limit { get; protected set; }

		public IEnumerable<dynamic> Data { get; protected set; }
	}

	public class PagedResult<TEntity> : PagedResult
	{
		public PagedResult(int page, int limit, int count, IEnumerable<TEntity> data)
		{
			Page = page;
			Limit = limit;
			Count = count;
			Data = data.Select(x => (dynamic) x);
		}
	}
}