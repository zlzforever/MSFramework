using System.Collections.Generic;

namespace MicroserviceFramework.FeatureManagement
{
	public interface IFeatureFinder
	{
		IEnumerable<Feature> GetAllList();
	}
}