using Microsoft.Extensions.DependencyInjection;

using NDE.VectorStore.HttpFactory;
using NDE.VectorStore.Interfaces;
using NDE.VectorStore.Qdrant;
using NDE.VectorStore.Weaviate;

namespace NDE.VectorStore.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddQdrantVectorStore(this IServiceCollection services, string endpoint, string? apiKey = null)
  {
    services.AddTransient<LoggingHandler>();
    services.AddHttpClient<IVectorStore, QdrantVectorStore>((serviceProvider, httpClient) =>
    {
      httpClient.BaseAddress = new Uri(endpoint);
      if (!string.IsNullOrWhiteSpace(apiKey))
        httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
    }).AddHttpMessageHandler<LoggingHandler>();

    return services;
  }

  public static IServiceCollection AddWeaviateVectorStore(this IServiceCollection services, string endpoint, string? apiKey = null)
  {
    services.AddHttpClient<IVectorStore, WeaviateVectorStore>((serviceProvider, httpClient) =>
    {
      httpClient.BaseAddress = new Uri(endpoint);
      if (!string.IsNullOrWhiteSpace(apiKey))
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }).AddHttpMessageHandler<LoggingHandler>();

    return services;
  }
}
