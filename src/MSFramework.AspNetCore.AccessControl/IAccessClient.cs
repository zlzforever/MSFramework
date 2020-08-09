using System.Net;
using System.Threading.Tasks;

namespace MSFramework.AspNetCore.AccessControl
{
	public interface IAccessClient
	{
		Task<(bool HasAccess, HttpStatusCode StatusCode)> HasAccessAsync(string subject, string @object, string action,
			string application);
	}
}