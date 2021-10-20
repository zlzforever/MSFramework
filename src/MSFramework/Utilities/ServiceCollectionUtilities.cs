using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Utilities
{
	public static class ServiceCollectionUtilities
	{
		public static void TryAdd(IServiceCollection collection, ServiceDescriptor serviceDescriptor)
		{
			Check.NotNull(collection, nameof(collection));
			Check.NotNull(serviceDescriptor, nameof(serviceDescriptor));

			foreach (var x in collection)
			{
				if (x == null)
				{
					continue;
				}

				if (x.ServiceType == serviceDescriptor.ServiceType &&
				    (
					    serviceDescriptor.ImplementationType != null &&
					    x.ImplementationType == serviceDescriptor.ImplementationType
					    || serviceDescriptor.ImplementationFactory != null &&
					    x.ImplementationFactory?.GetHashCode() ==
					    serviceDescriptor.ImplementationFactory.GetHashCode()
					    || serviceDescriptor.ImplementationInstance != null && x.ImplementationInstance ==
					    serviceDescriptor.ImplementationInstance
				    ) &&
				    x.Lifetime == serviceDescriptor.Lifetime)
				{
					return;
				}
			}

			collection.Add(serviceDescriptor);
		}
	}
}