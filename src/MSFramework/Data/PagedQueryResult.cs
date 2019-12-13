using System.Collections.Generic;

namespace MSFramework.Data
{
	public class PagedQueryResult<TEntity>
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
		public List<TEntity> Entities { get; set; }
	}
}