using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IUnitOfWork
	{	
		/// <summary>
		/// 提交当前上下文的事务更改
		/// </summary>
		void Commit();

		Task CommitAsync();
		
		/// <summary>
		/// 对数据库连接开启事务
		/// </summary>
		void BeginOrUseTransaction();

		/// <summary>
		/// 对数据库连接开启事务
		/// </summary>
		/// <param name="cancellationToken">异步取消标记</param>
		/// <returns></returns>
		Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default);
	}
}