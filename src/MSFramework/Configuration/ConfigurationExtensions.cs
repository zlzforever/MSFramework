using System;
using Microsoft.Extensions.Configuration;

namespace MicroserviceFramework.Configuration
{
	public static class ConfigurationExtensions
	{
		public static void Print(this IConfiguration configuration, Action<string> writer)
		{
			if (configuration == null || writer == null)
			{
				return;
			}

			foreach (var kv in configuration.GetChildren())
			{
				if (!string.IsNullOrWhiteSpace(kv.Key))
				{
					writer($"Configuration: {kv.Key} = {kv.Value}");
				}
			}
		}
	}
}