using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MSFramework.Data;
using Newtonsoft.Json;

namespace MSFramework.Http
{
	public class ApiClient
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public ApiClient(IHttpClientFactory httpClientFactory)
		{
			httpClientFactory.NotNull(nameof(httpClientFactory));
			_httpClientFactory = httpClientFactory;
		}

		public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string clientName, string url)
		{
			var httpClient = _httpClientFactory.CreateClient(clientName);

			var request = new HttpRequestMessage(HttpMethod.Get, url);
			var response = await httpClient.SendAsync(request);

			var result = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<ApiResult<TResponse>>(result);
		}

		public async Task<ApiResult<TResponse>> PostAsync<TResponse>(string clientName, string url, dynamic data)
		{
			var httpClient = _httpClientFactory.CreateClient(clientName);
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			if (data != null)
			{
				request.Content =
					new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			}

			var response = await httpClient.SendAsync(request);

			var result = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<ApiResult<TResponse>>(result);
		}

		public async Task<bool> PostAsync(string clientName, string url, dynamic data)
		{
			var result = await PostAsync<ApiResult>(clientName, url, data);
			return result.Success;
		}

		public async Task<ApiResult<TResponse>> PutAsync<TResponse>(string clientName, string url, dynamic data)
		{
			var httpClient = _httpClientFactory.CreateClient(clientName);

			var request = new HttpRequestMessage(HttpMethod.Put, url);
			if (data != null)
			{
				request.Content =
					new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			}

			var response = await httpClient.SendAsync(request);

			var result = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<ApiResult<TResponse>>(result);
		}

		public async Task<bool> PutAsync(string clientName, string url, dynamic data)
		{
			var result = await PutAsync<ApiResult>(clientName, url, data);
			return result.Success;
		}

		public async Task<bool> HeadAsync(string clientName, string url)
		{
			var httpClient = _httpClientFactory.CreateClient(clientName);
			var request = new HttpRequestMessage(HttpMethod.Head, url);
			var response = await httpClient.SendAsync(request);

			return response.StatusCode == HttpStatusCode.OK;
		}

		public async Task<HttpResponseMessage> SendAsync(string clientName, HttpRequestMessage message)
		{
			var httpClient = _httpClientFactory.CreateClient(clientName);
			var response = await httpClient.SendAsync(message);
			return response;
		}

		public async Task<ApiResult<TResponse>> DeleteAsync<TResponse>(string clientName, string url)
		{
			var httpClient = _httpClientFactory.CreateClient(clientName);
			var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));
			var str = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<ApiResult<TResponse>>(str);
			return result;
		}

		public async Task<bool> DeleteAsync(string clientName, string url)
		{
			var result = await DeleteAsync<ApiResult>(clientName, url);
			return result.Success;
		}
	}
}