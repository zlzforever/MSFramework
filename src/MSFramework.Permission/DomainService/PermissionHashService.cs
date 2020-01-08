using System.Threading.Tasks;
using MSFramework.Extensions;

namespace MSFramework.Permission.DomainService
{
	public class PermissionHashService : IPermissionHashService
	{
		public string Compute(string name, string type, string service, string path)
		{
			string data;
			switch (type)
			{
				case "Api":
				{
					data = $"{service}_{path}";
					break;
				}
				default:
				{
					data = $"{name}_{type}_{service}_{path}";
					break;
				}
			}

			return data.ToMd5();
		}
	}
}