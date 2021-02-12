using System.Collections.Generic;
using System.Threading.Tasks;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.FeatureManagement
{
	public interface IFeatureRepository : IRepository<Feature>, IScopeDependency
	{
		Feature GetByName(string name);

		IEnumerable<Feature> GetAllList();

		bool IsAvailable();
	}
}