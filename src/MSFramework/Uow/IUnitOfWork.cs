using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Uow
{
	/// <summary>
	/// 业务单元操作接口
	/// </summary>
	public interface IUnitOfWork
	{

		void Rollback();

		Task RollbackAsync();
		/// <summary>
		/// 提交当前上下文的事务更改
		/// </summary>
		void Commit();

		Task CommitAsync();
	}
}
