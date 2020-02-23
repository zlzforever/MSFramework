using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApplicationException = MSFramework.Application.ApplicationException;

namespace MSFramework.AspNetCore.Permission
{
	public class CerberusClient : ICerberusClient
	{
		class CerberusResult
		{
			public int Code { get; set; }
			public bool Success { get; set; }

			public string Msg { get; set; }
			public object Data { get; set; }
		}

		private readonly PermissionOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _cache;

		public CerberusClient(IMemoryCache cache, PermissionOptions options, IHttpClientFactory httpClientFactory)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
			_cache = cache;
		}

		public async Task<bool> ExistsAsync(string serviceId)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services/{serviceId}";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Head, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.CerberusSecurityHeader);
			var response = await client.SendAsync(request);
			return response.StatusCode == HttpStatusCode.OK;
		}

		public async Task<bool> HasPermissionAsync(string userId, string serviceId, string identification)
		{
			var key = $"{userId}_{serviceId}_{identification}";
			if (_cache.TryGetValue(key, out object cacheValue))
			{
				return (bool) cacheValue;
			}

			var queryParam = $"identification={identification}";
			var url =
				$"{_options.Cerberus}/api/v1.0/users/{userId}/services/{serviceId}/permissions?" + queryParam;
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Head, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.CerberusSecurityHeader);
			var response = await client.SendAsync(request);
			var hasPermission = response.StatusCode == HttpStatusCode.OK;
			_cache.Set(key, hasPermission, new TimeSpan(0, 0, _options.CacheTTL, 0));
			return hasPermission;
		}

		public async Task AddPermissionAsync(string serviceId, Permission permission)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services/{serviceId}/permissions";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.CerberusSecurityHeader);
			request.Content =
				new StringContent(JsonConvert.SerializeObject(permission), Encoding.UTF8, "application/json");
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}
		}

		public async Task<List<Permission>> GetPermissionsAsync(string serviceId)
		{
			var client = _httpClientFactory.CreateClient("Cerberus");
			var url = $"{_options.Cerberus}/api/v1.0/services/{serviceId}/permissions";
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.CerberusSecurityHeader);
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}

			return ((JArray) result.Data).ToObject<List<Permission>>();
		}

		public async Task RenewalAsync(string serviceId, string ids)
		{
			var url =
				$"{_options.Cerberus}/api/v1.0/services/{serviceId}/permissions/{ids}/renewal";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Put, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.CerberusSecurityHeader);
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}
		}

		public async Task ExpireAsync(string serviceId, string ids)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services/{serviceId}/permissions/{ids}/expire";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Put, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.CerberusSecurityHeader);
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}
		}
	}
}