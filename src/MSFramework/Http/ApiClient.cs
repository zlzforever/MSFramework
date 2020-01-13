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

		public async Task<ApiResponse<T>> GetAsync<T>(string url) where T : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.GetStringAsync(url);
			var response = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
			return response;
		}

		public async Task<ApiResponse<T>> PostAsync<T>(string url, dynamic data) where T : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.PostAsync(url,
				new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
			var result = await content.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<ApiResponse<T>>(result);
			return response;
		}

		public async Task<ApiResponse<T>> PutAsync<T>(string url, dynamic data) where T : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.PutAsync(url,
				new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
			var result = await content.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<ApiResponse<T>>(result);
			return response;
		}

		public async Task<ApiResponse<T>> DeleteAsync<T>(string url, dynamic data) where T : class
		{
			var httpClient = await GetHttpClient(url);
			var content = await httpClient.DeleteAsync(url);
			var result = await content.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<ApiResponse<T>>(result);
			return response;
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