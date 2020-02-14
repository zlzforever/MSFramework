using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MSFramework.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

		public CerberusClient(PermissionOptions options, IHttpClientFactory httpClientFactory)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
		}

		public async Task CreateService(string service)
		{
			var url = $"{_options.Cerberus}/api/v1.0/services";
			var client = _httpClientFactory.CreateClient("Cerberus");
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.Headers.TryAddWithoutValidation("SecurityHeader", _options.SecurityHeader);
			request.Content = new StringContent(JsonConvert.SerializeObject(new
			{
				Name = service
			}), Encoding.UTF8, "application/json");
			var response = await client.SendAsync(request);
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<CerberusResult>(str);
			if (!result.Success && result.Msg != "Service exists")
			{
				throw new ApplicationException(result.Msg);
			}
		}

		public async Task<bool> HasPermissionAsync(string userId, string service, string identification)
		{
			var url =
				$"{_options.Cerberus}/api/v1.0/users/{userId}/services/{service}/permissions/{WebUtility.UrlEncode(identification)}";
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

			return result.Data.ToString() == "true";
		}

		public async Task AddPermissionAsync(Permission permission)
		{
			var url = $"{_options.Cerberus}/api/v1.0/permissions";
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
			var url = $"{_options.Cerberus}/api/v1.0/services/{service}/permissions";
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

		public async Task RenewalAsync(string ids)
		{
			var url = $"{_options.Cerberus}/api/v1.0/permissions/{ids}/renewal";
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

		public async Task ExpireAsync(string ids)
		{
			var url = $"{_options.Cerberus}/api/v1.0/permissions/{ids}/expire";
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