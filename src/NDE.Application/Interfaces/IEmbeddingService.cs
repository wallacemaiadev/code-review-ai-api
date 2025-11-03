namespace NDE.Application.Interfaces;

public interface IEmbeddingService
{
  Task<float[]> GenerateEmbedding(string text, CancellationToken ct = default);
}
