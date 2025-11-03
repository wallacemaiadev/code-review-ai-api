using NDE.VectorStore.DTOs;

namespace NDE.VectorStore.Interfaces;

public interface IVectorStore
{
  Task<bool> EnsureCollectionAsync(string name);
  Task<bool> UpsertAsync<T>(string collection, Guid id, float[] vector, T payload, bool wait = true);
  Task<IReadOnlyList<VectorSearchResult<T>>> SearchAsync<T>(string collection, float[] query, int topK, object? filter = null);
  Task<bool> ExistsAsync(string collection, Guid id);
}
