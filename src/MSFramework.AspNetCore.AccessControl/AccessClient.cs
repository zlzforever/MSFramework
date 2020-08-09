using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace MSFramework.AspNetCore.AccessControl
{
	public class AccessClient : IAccessClient
	{
		private readonly AccessControlOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _cache;
		private readonly IHttpContextAccessor _accessor;

		public AccessClient(IMemoryCache cache,
			AccessControlOptions options,
			IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
			_accessor = accessor;
			_cache = cache;
		}

		public async Task<(bool HasAccess, HttpStatusCode StatusCode)> HasAccessAsync(string subject, string @object,
			string action, string application)
		{
			var key = $"{subject}{@object}{action}";

			return await _cache.GetOrCreateAsync(key, async (entry) =>
			{
				var queryParam = $"subject={subject}&object={@object}&action={action}&application={application}";
				var url = $"{_options.ServiceUrl}/api/v1.0/access?{queryParam}";
				var client = _httpClientFactory.CreateClient(GetType().Name);
				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, url);
				var accessToken = await _accessor.HttpContext.GetTokenAsync("access_token");
				httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", $"Bear {accessToken}");
				var response = await client.SendAsync(httpRequestMessage);
				var hasPermission = response.StatusCode == HttpStatusCode.OK;
				var result = hasPermission ? (true, HttpStatusCode.OK) : (false, response.StatusCode);
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheTTL);
				entry.Value = result;
				return result;
			});
		}
	}
}