using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Data;
using MSFramework.Extensions;
using Newtonsoft.Json;

namespace MSFramework.Http
{
	public class ApiClient
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IBearProvider _bearProvider;

		public ApiClient(IServiceProvider serviceProvider)
		{
			serviceProvider.NotNull(nameof(serviceProvider));
			var serviceProvider1 = serviceProvider;
			_httpClientFactory = serviceProvider1.GetRequiredService<IHttpClientFactory>();
			_bearProvider = serviceProvider1.GetRequiredService<IBearProvider>();
		}

		public async Task<TResponse> GetAsync<TResponse, TDataEntity>(string url)
			where TResponse : ApiResult<TDataEntity>
			where TDataEntity : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.GetStringAsync(url);
			var response = JsonConvert.DeserializeObject<TResponse>(content);
			return response;
		}

		public async Task<TResponse> PostAsync<TResponse, TDataEntity>(string url, dynamic data)
			where TResponse : ApiResult<TDataEntity>
			where TDataEntity : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.PostAsync(url,
				new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
			var result = await content.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<TResponse>(result);
			return response;
		}

		public async Task<bool> PostAsync(string url, dynamic data)
		{
			var result = await PostAsync<ApiResult, object>(url, data);
			return result.Success;
		}

		public async Task<TResponse> PutAsync<TResponse, TDataEntity>(string url, dynamic data)
			where TResponse : ApiResult<TDataEntity>
			where TDataEntity : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.PutAsync(url,
				new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
			var result = await content.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<TResponse>(result);
			return response;
		}

		public async Task<bool> PutAsync(string url, dynamic data)
		{
			var result = await PutAsync<ApiResult, object>(url, data);
			return result.Success;
		}

		public async Task<TResponse> DeleteAsync<TResponse, TDataEntity>(string url)
			where TResponse : ApiResult<TDataEntity>
			where TDataEntity : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.DeleteAsync(url);
			var result = await content.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<TResponse>(result);
			return response;
		}

		public async Task<bool> DeleteAsync(string url)
		{
			var result = await DeleteAsync<ApiResult, object>(url);
			return result.Success;
		}

		private async Task<HttpClient> GetHttpClient(string url)
		{
			url.NotNull(nameof(url));
			var uri = new Uri(url);
			var accessToken = await _bearProvider.GetTokenAsync();
			var httpClient = _httpClientFactory.CreateClient(uri.Host);
			if (!accessToken.IsNullOrWhiteSpace())
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			return httpClient;
		}
	}
}