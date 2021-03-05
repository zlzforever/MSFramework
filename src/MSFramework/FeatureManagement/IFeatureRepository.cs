using System.Collections.Generic;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.FeatureManagement
{
	public interface IFeatureRepository : IRepository<Feature, ObjectId>, IScopeDependency
	{
		Feature GetByName(string name);

		IEnumerable<Feature> GetAllList();

		bool IsAvailable();
	}
}