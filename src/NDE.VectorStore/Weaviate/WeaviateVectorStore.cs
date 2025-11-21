

using NDE.VectorStore.DTOs;
using NDE.VectorStore.Interfaces;

namespace NDE.VectorStore.Weaviate;

public class WeaviateVectorStore : IVectorStore
{
  public Task<bool> EnsureCollectionAsync(string name)
  {
    throw new NotImplementedException();
  }

  public Task<bool> ExistsAsync(string collection, Guid id)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyList<VectorSearchResult<T>>> SearchAsync<T>(string collection, float[] query, int topK, object? filter = null)
  {
    throw new NotImplementedException();
  }

  public Task<bool> UpdateVerdictAsync(string collection, Guid id, string verdict, int weight)
  {
    throw new NotImplementedException();
  }

  public Task<bool> UpsertAsync<T>(string collection, Guid id, float[] vector, T payload, bool wait = true)
  {
    throw new NotImplementedException();
  }
}
