using System.ClientModel;
using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NDE.Application.Interfaces;
using NDE.Domain.Utils;
using NDE.Observability.Metrics;
using NDE.VectorStore.Options;

using OpenAI;
using OpenAI.Embeddings;

namespace NDE.Application.Services;

public class EmbeddingService : IEmbeddingService
{
  private readonly AppSettings _settings;
  private readonly ILogger<EmbeddingService> _logger;
  private readonly EmbeddingClient _embeddingClient;
  private readonly VectorStoreOptions _vectorStore;
  private readonly int EmbeddingDim;

  public EmbeddingService(IOptions<AppSettings> settings, IOptions<VectorStoreOptions> vectorStore, ILogger<EmbeddingService> logger)
  {
    _settings = settings.Value;
    _logger = logger;
    _embeddingClient = new EmbeddingClient(
      model: _settings.EmbeddingService.ModelName,
      credential: new ApiKeyCredential(_settings.EmbeddingService.ApiKey),
      options: new OpenAIClientOptions
      {
        Endpoint = new Uri(_settings.EmbeddingService.Endpoint)
      });
    _vectorStore = vectorStore.Value;
    EmbeddingDim = _vectorStore.Dimensions;
  }

  public async Task<float[]?> GenerateEmbedding(string text, CancellationToken ct = default)
  {
    if (string.IsNullOrWhiteSpace(text))
      throw new ArgumentException("O texto para geração do embedding não pode ser nulo ou vazio.", nameof(text));

    var watch = Stopwatch.StartNew();

    _logger.LogInformation("Gerando embedding do texto com {Length} caracteres.", text.Length);

    try
    {
      var embedding = await _embeddingClient.GenerateEmbeddingAsync(
        input: text,
        options: new EmbeddingGenerationOptions
        {
          Dimensions = EmbeddingDim
        },
        cancellationToken: ct);

      var vector = embedding.Value.ToFloats().ToArray();
      var dim = vector.Length;

      _logger.LogInformation("Embedding gerado (dim={Dim})", dim);

      if (EmbeddingDim > 0 && dim != EmbeddingDim)
      {
        var msg = $"Dimensão do embedding ({dim}) difere da configurada ({EmbeddingDim}).";
        _logger.LogWarning(msg);
        throw new InvalidOperationException(msg);
      }

      watch.Stop();
      AppMetrics.ObserveGenerateEmbeddingDuration(watch.Elapsed);

      return vector;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Falha ao gerar embedding");
      return null;
    }
  }
}
