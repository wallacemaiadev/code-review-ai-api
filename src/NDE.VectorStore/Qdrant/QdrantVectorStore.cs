using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Options;

using NDE.VectorStore.DTOs;
using NDE.VectorStore.HttpFactory;
using NDE.VectorStore.Interfaces;
using NDE.VectorStore.Models;
using NDE.VectorStore.Options;

namespace NDE.VectorStore.Qdrant;

public class QdrantVectorStore : HttpServiceBase, IVectorStore
{
  private readonly VectorStoreOptions _options;

  public QdrantVectorStore(IOptions<VectorStoreOptions> options, HttpClient httpClient) : base(httpClient)
  {
    _options = options.Value;
  }

  public async Task<bool> EnsureCollectionAsync(string name)
  {
    var exists = await GetAsync<JsonElement>($"/collections/{name}");
    if (exists.ValueKind == JsonValueKind.Undefined)
    {
      var body = new
      {
        vectors = new
        {
          size = _options.Dimensions,
          distance = "Cosine"
        }
      };

      await PutAsync($"/collections/{name}", body);
    }

    return true;
  }

  public async Task<bool> UpsertAsync<T>(string collection, Guid id, float[] vector, T payload, bool wait = true)
  {
    if (string.IsNullOrWhiteSpace(collection))
      throw new ArgumentException("Collection inválida.", nameof(collection));
    if (id.Equals(Guid.Empty))
      throw new ArgumentException("Id inválido.", nameof(id));
    if (vector is null || vector.Length == 0)
      throw new ArgumentException("Vetor não pode ser nulo ou vazio.", nameof(vector));

    await EnsureCollectionAsync(collection);

    var body = new
    {
      points = new[]
      {
        new {
          id = id.ToString(),
          vector = vector,
          payload = payload
        }
      }
    };

    await PutAsync($"/collections/{collection}/points?wait={wait.ToString().ToLower()}", body);

    return true;
  }

  public async Task<IReadOnlyList<VectorSearchResult<T>>> SearchAsync<T>(string collection, float[] query, int topK, object? filter = null)
  {
    var body = new Dictionary<string, object?>
    {
      ["vector"] = query,
      ["limit"] = topK,
      ["with_payload"] = true
    };

    if (filter != null)
      body["filter"] = filter;

    await EnsureCollectionAsync(collection);

    var response = await Http.PostAsJsonAsync($"/collections/{collection}/points/search", body);

    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadFromJsonAsync<JsonElement>();

    var list = new List<VectorSearchResult<T>>();
    foreach (var it in json!.GetProperty("result").EnumerateArray())
    {
      var score = it.GetProperty("score").GetDouble();
      var payloadJson = it.GetProperty("payload").GetRawText();

      var payload = JsonSerializer.Deserialize<T>(
          payloadJson,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
      )!;

      if (payload is CodeReviewSuggestion crs)
        score *= crs.Weight;

      list.Add(new VectorSearchResult<T>(score, payload));
    }

    return list;
  }

  public async Task<bool> ExistsAsync(string collection, Guid id)
  {
    var response = await GetAsync<JsonElement>($"/collections/{collection}/points/{id}");
    return response.ValueKind != JsonValueKind.Undefined &&
           response.TryGetProperty("result", out var result) &&
           result.ValueKind != JsonValueKind.Null;
  }
}
