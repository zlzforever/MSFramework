using System;
using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IUnitOfWorkManager : IDisposable
	{
		void Commit();

		Task CommitAsync();
	}
}