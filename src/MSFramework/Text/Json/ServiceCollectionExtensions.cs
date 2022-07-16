using System.Text.Json;
using MicroserviceFramework.Serialization;

namespace MicroserviceFramework.Text.Json
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static MicroserviceFrameworkBuilder UseDefaultJsonHelper(this MicroserviceFrameworkBuilder builder,
			JsonSerializerOptions options = null)
		{
			Default.JsonHelper = options == null ? JsonHelper.Create() : new JsonHelper(options);

			return builder;
		}
	}
}