using System;
using System.Threading;
using System.Threading.Tasks;

namespace MSFramework.Domain
{
	public interface IUnitOfWork : IDisposable
	{
		Task CommitAsync();
	}
}