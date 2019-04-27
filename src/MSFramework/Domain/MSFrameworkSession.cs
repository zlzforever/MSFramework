using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MSFramework.Domain
{
	public interface IMSFrameworkSession
	{
		string UserId { get; }

		Task CommitAsync();

		void Commit();
	}
}