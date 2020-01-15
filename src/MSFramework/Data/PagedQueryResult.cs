using System.Collections.Generic;
using System.Linq;

namespace MSFramework.Data
{
	public interface IPagedQueryResult
	{
		/// <summary>
		/// 总计
		/// </summary>
		int Total { get; set; }

		/// <summary>
		/// 当前页数 
		/// </summary>
		int Page { get; set; }

		/// <summary>
		/// 每页数据量 
		/// </summary>
		int Limit { get; set; }

		/// <summary>
		/// 当前页结果
		/// </summary>
		IEnumerable<dynamic> GetEntities();
	}

	public class PagedQueryResult<TEntity> : IPagedQueryResult
	{
		/// <summary>
		/// 总计
		/// </summary>
		public int Total { get; set; }

		/// <summary>
		/// 当前页数 
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// 每页数据量 
		/// </summary>
		public int Limit { get; set; }

		/// <summary>
		/// 当前页结果
		/// </summary>
		public List<TEntity> Data { get; set; }

		public IEnumerable<dynamic> GetEntities() => Data.Select(x => (dynamic) x);
	}
}