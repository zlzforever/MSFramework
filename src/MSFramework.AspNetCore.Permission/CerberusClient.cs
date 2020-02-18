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
	class CerberusResult
	{
		public int Code { get; set; }
		public bool Success { get; set; }

		public string Msg { get; set; }
		public object Data { get; set; }
	}

	public class CerberusClient
	{
		private readonly PermissionOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _cache;

		public CerberusClient(IMemoryCache cache, PermissionOptions options, IHttpClientFactory httpClientFactory)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
			_cache = cache;
		}

		public async Task<bool> ExistsAsync(string service)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services/{WebUtility.UrlEncode(service)}";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Head, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
			var response = await client.SendAsync(request);
			return response.StatusCode == HttpStatusCode.OK;
		}

		public async Task<bool> HasPermissionAsync(string userId, string service, string identification)
		{
			var key = $"{userId}_{service}_{identification}";
			if (_cache.TryGetValue(key, out object cacheValue))
			{
				return (bool) cacheValue;
			}

			var url =
				$"{_options.Cerberus}/api/v1.0/users/{userId}/services/{WebUtility.UrlEncode(service)}/permissions/{WebUtility.UrlEncode(identification)}";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}

			var hasPermission = result.Data?.ToString().ToLower() == "true";
			_cache.Set(key, hasPermission, new TimeSpan(0, 0, _options.CahceTTL, 0));
			return hasPermission;
		}

		public async Task AddPermissionAsync(string service, Permission permission)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services/{WebUtility.UrlEncode(service)}/permissions";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
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

		public async Task<List<Permission>> GetPermissionsAsync(string service)
		{
			var client = _httpClientFactory.CreateClient("Cerberus");
			var url = $"{_options.Cerberus}/api/v1.0/services/{WebUtility.UrlEncode(service)}/permissions";
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}

			return ((JArray) result.Data).ToObject<List<Permission>>();
		}

		public async Task RenewalAsync(string service, string ids)
		{
			var url =
				$"{_options.Cerberus}/api/v1.0/services/{WebUtility.UrlEncode(service)}/permissions/{ids}/renewal";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Put, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success)
			{
				throw new ApplicationException(result.Msg);
			}
		}

		public async Task ExpireAsync(string service, string ids)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services/{WebUtility.UrlEncode(service)}/permissions/{ids}/expire";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Put, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
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