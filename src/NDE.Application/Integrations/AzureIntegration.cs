
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using NDE.Application.HttpFactory;

namespace NDE.Application.Integrations.Azure;

public class AzureIntegration : HttpServiceBase, IAzureIntegration
{
  private readonly ILogger<AzureIntegration> _logger;

  public AzureIntegration(HttpClient httpClient, ILogger<AzureIntegration> logger) : base(httpClient)
  {
    _logger = logger;
  }

  public async Task<bool> CommentOnThread(string baseUrl)
  {
    var url = $"{baseUrl}/comments?api-version=7.1-preview.1";

    var body = new
    {
      content = $"Code Review atualizado {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
      commentType = 1
    };

    try
    {
      var response = await Http.PostAsJsonAsync(url, body);

      if (response.IsSuccessStatusCode)
        return true;

      var content = await response.Content.ReadAsStringAsync();
      _logger.LogError($"Houve um erro ao comentar na thread: {response.StatusCode} - {content}");
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError($"Exceção ao comentar na thread: {ex.Message}");
      return true;
    }
  }
}
