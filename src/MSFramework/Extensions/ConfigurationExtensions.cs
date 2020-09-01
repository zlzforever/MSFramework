using System;
using Microsoft.Extensions.Configuration;

namespace MicroserviceFramework.Extensions
{
	public static class ConfigurationExtensions
	{
		public static void Print(this IConfiguration configuration, Action<string> printer)
		{
			if (configuration == null || printer == null)
			{
				return;
			}

			printer("Configuration: ");
			foreach (var kv in configuration.GetChildren())
			{
				if (!string.IsNullOrWhiteSpace(kv.Key))
				{
					printer($"{kv.Key} = {kv.Value}");
				}
			}
		}
	}
}