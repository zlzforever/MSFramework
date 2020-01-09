using System;
using Microsoft.Extensions.Configuration;

namespace MSFramework.Extensions
{
	public static class ConfigurationExtensions
	{
		public static void Print(this IConfiguration configuration)
		{
			if (configuration == null)
			{
				return;
			}

			Console.WriteLine("Configuration: ");
			foreach (var kv in configuration.GetChildren())
			{
				Console.WriteLine($"{kv.Key} = {kv.Value}");
			}
		}
	}
}