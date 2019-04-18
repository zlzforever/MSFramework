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
	}
}