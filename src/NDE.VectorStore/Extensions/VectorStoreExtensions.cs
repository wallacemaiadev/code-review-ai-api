using NDE.VectorStore.DTOs;
using NDE.VectorStore.Interfaces;

namespace NDE.VectorStore.Extensions;

public static class VectorStoreExtensions
{

  public static async Task<IReadOnlyList<VectorSearchResult<T>>> SmartSearchAsync<T>(
  this IVectorStore store,
  string collection,
  float[] query,
  Dictionary<string, string> filters,
  int topK,
  double minScore = 1)
  {
    object? filterLocal = null;

    if (filters is { Count: > 0 })
    {
      var must = filters
          .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
          .Select(kv => new
          {
            key = kv.Key,
            match = new { value = kv.Value }
          })
          .ToArray();

      if (must.Length > 0)
        filterLocal = new { must };
    }

    var local = await store.SearchAsync<T>(collection, query, topK, filterLocal);
    if (local.Any() && local.Max(x => x.Score) >= minScore)
      return local;


    return Array.Empty<VectorSearchResult<T>>();
  }
}