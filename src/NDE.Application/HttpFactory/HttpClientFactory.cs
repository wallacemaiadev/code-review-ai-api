using System.Net.Http.Json;
using System.Text.Json;

namespace NDE.Application.HttpFactory;

public abstract class HttpServiceBase
{
  protected readonly HttpClient Http;

  protected HttpServiceBase(HttpClient httpClient)
  {
    Http = httpClient;
  }

  protected async Task<T?> GetAsync<T>(string url)
  {
    var response = await Http.GetAsync(url);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });
  }

  protected async Task<T?> PostAsync<T>(string url, object body)
  {
    var response = await Http.PostAsJsonAsync(url, body);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<T>();
  }

  protected async Task<HttpResponseMessage> PutAsync(string url, object body)
  {
    var response = await Http.PutAsJsonAsync(url, body);
    response.EnsureSuccessStatusCode();
    return response;
  }
}