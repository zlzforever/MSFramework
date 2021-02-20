using System;
using System.Collections.Generic;
using MicroserviceFramework.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.DependencyInjection
{
	public class ScopedServiceResolver : IScopedServiceResolver
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public ScopedServiceResolver(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public T GetService<T>()
		{
			return _httpContextAccessor.HttpContext == null
				? default
				: _httpContextAccessor.HttpContext.RequestServices.GetService<T>();
		}

		public object GetService(Type serviceType)
		{
			return _httpContextAccessor.HttpContext?.RequestServices.GetService(serviceType);
		}

		public IEnumerable<T> GetServices<T>()
		{
			return _httpContextAccessor.HttpContext?.RequestServices.GetServices<T>();
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _httpContextAccessor.HttpContext?.RequestServices.GetServices(serviceType);
		}
	}
}