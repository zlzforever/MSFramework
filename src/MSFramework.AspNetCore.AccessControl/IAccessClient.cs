using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
	public interface IAccessClient
	{
		Task<(bool HasAccess, HttpStatusCode StatusCode)> HasAccessAsync(string subject, string @object, string action,
			string application);

		Task<Dictionary<string, List<ApiInfo>>> GetAllListAsync(string application);
		Task CreateAsync(ApiInfo apiInfo);
		Task RenewalAsync(string id);
		Task ObsoleteAsync(string id);
	}
}