using System;
using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IUnitOfWork : IDisposable
	{
		void Commit();

		Task CommitAsync();
	}
}