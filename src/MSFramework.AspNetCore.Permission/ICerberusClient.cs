using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFramework.AspNetCore.Permission
{
	public interface ICerberusClient
	{
		Task<bool> ExistsAsync(string serviceId);

		Task<bool> HasPermissionAsync(string userId, string serviceId, string identification);

		Task AddPermissionAsync(string serviceId, Permission permission);

		Task<List<Permission>> GetPermissionsAsync(string serviceId);

		Task RenewalAsync(string serviceId, string ids);

		Task ExpireAsync(string serviceId, string ids);
		Task<PermissionData> GetPermissionAsync(string userId, string serviceId, string identification);
	}
}