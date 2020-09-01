using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroserviceFramework.AspNetCore.AccessControl
{
	public class AccessClient : IAccessClient
	{
		private readonly AccessControlOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _cache;

		public AccessClient(IMemoryCache cache,
			AccessControlOptions options,
			IHttpClientFactory httpClientFactory)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
			_cache = cache;
		}

		public async Task<(bool HasAccess, HttpStatusCode StatusCode)> HasAccessAsync(string subject, string @object,
			string action, string application)
		{
			var key = $"{subject}{@object}{action}";

			return await _cache.GetOrCreateAsync(key, async (entry) =>
			{
				var queryParam = $"subject={subject}&object={@object}&action={action}&application={application}";
				var url = $"{_options.ServiceUrl}/v1.1/access?{queryParam}";
				var client = _httpClientFactory.CreateClient(_options.HttpClient);
				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, url);
				var response = await client.SendAsync(httpRequestMessage);
				var hasPermission = response.StatusCode == HttpStatusCode.OK;
				var result = hasPermission ? (true, HttpStatusCode.OK) : (false, response.StatusCode);
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheTTL);
				entry.Value = result;
				return result;
			});
		}

		public async Task<Dictionary<string, List<ApiInfo>>> GetAllListAsync(string application)
		{
			var queryParam = $"application={application}";
			var url = $"{_options.ServiceUrl}/v1.1/api-infos?{queryParam}";
			var client = _httpClientFactory.CreateClient(_options.HttpClient);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
			var response = await client.SendAsync(httpRequestMessage);
			response.EnsureSuccessStatusCode();
			var str = await response.Content.ReadAsStringAsync();
			var json = JObject.Parse(str);
			if (json["success"].ToObject<bool>())
			{
				return json["data"].ToObject<Dictionary<string, List<ApiInfo>>>();
			}
			else
			{
				throw new MicroserviceFrameworkException("Query exist api-info failed");
			}
		}

		public async Task CreateAsync(ApiInfo apiInfo)
		{
			var url = $"{_options.ServiceUrl}/v1.1/api-infos";
			var client = _httpClientFactory.CreateClient(_options.HttpClient);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(JsonConvert.SerializeObject(new
				{
					apiInfo.Name,
					apiInfo.Application,
					apiInfo.Description,
					apiInfo.Group
				}), Encoding.UTF8, "application/json")
			};
			var response = await client.SendAsync(httpRequestMessage);
			response.EnsureSuccessStatusCode();
			var str = await response.Content.ReadAsStringAsync();
			var json = JObject.Parse(str);
			if (!json["success"].ToObject<bool>())
			{
				var msg = json["message"].ToString();
				throw new MicroserviceFrameworkException($"Create api-info failed: {msg}");
			}
		}

		public async Task RenewalAsync(string id)
		{
			var url = $"{_options.ServiceUrl}/v1.1/api-infos/{id}/renewal";
			var client = _httpClientFactory.CreateClient(_options.HttpClient);
			var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url);
			var response = await client.SendAsync(httpRequestMessage);
			response.EnsureSuccessStatusCode();
		}

		public async Task ObsoleteAsync(string id)
		{
			var url = $"{_options.ServiceUrl}/v1.1/api-infos/{id}/obsolete";
			var client = _httpClientFactory.CreateClient(_options.HttpClient);
			var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url);
			var response = await client.SendAsync(httpRequestMessage);
			response.EnsureSuccessStatusCode();
		}
	}
}