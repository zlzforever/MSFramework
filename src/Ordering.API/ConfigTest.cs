using AutoMapper.Configuration;
using MicroserviceFramework.Configuration;
using Microsoft.Extensions.Options;

namespace Ordering.API
{
	[Options]
	public class ConfigTest
	{
		public string Authority { get; set; }
	}
}