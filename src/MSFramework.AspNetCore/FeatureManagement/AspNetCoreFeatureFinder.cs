using System;
using System.Collections.Generic;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.FeatureManagement;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.FeatureManagement
{
	public class AspNetCoreFeatureFinder : IFeatureFinder
	{
		private readonly IServiceProvider _services;

		public AspNetCoreFeatureFinder(IServiceProvider serviceProvider)
		{
			_services = serviceProvider;
		}

		public IEnumerable<Feature> GetAllList()
		{
			var actionDescriptorCollectionProvider =
				_services.GetRequiredService<IActionDescriptorCollectionProvider>();
			var features = new List<Feature>();
			foreach (var actionDescriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
			{
				var (name, description) = actionDescriptor.GetFeature();
				features.Add(new Feature(name, description));
			}

			return features;
		}
	}
}