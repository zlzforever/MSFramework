using MSFramework.Domain;

namespace MSFramework.Permission.DomainService
{
	public interface IPermissionHashService : IDomainService
	{
		string Compute(string name, string type, string service, string path);
	}
}