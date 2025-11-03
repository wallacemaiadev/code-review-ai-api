using System.ClientModel;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NDE.Application.Interfaces;
using NDE.Domain.Utils;
using NDE.VectorStore.Options;

using OpenAI;
using OpenAI.Embeddings;

using SharpToken;

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
      model: _settings.OpenAI.ModelName,
      credential: new ApiKeyCredential(_settings.OpenAI.ApiKey),
      options: new OpenAIClientOptions
      {
        Endpoint = new Uri(_settings.OpenAI.Endpoint)
      });
    _vectorStore = vectorStore.Value;
    EmbeddingDim = _vectorStore.Dimensions;
  }

  public async Task<float[]> GenerateEmbedding(string text, CancellationToken ct = default)
  {
    if (string.IsNullOrWhiteSpace(text))
      throw new ArgumentException("O texto para geração do embedding não pode ser nulo ou vazio.", nameof(text));

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

      var model = "text-embedding-3-small";
      var tokens = CountEmbeddingTokens(text, model);

      var vector = embedding.Value.ToFloats().ToArray();
      var dim = vector.Length;

      _logger.LogInformation("Embedding gerado (dim={Dim})", dim);

      if (EmbeddingDim > 0 && dim != EmbeddingDim)
      {
        var msg = $"Dimensão do embedding ({dim}) difere da configurada ({EmbeddingDim}).";
        _logger.LogWarning(msg);
        throw new InvalidOperationException(msg);
      }

      return vector;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Falha ao gerar embedding.");
      throw;
    }
  }

  public int CountEmbeddingTokens(string text, string model = "text-embedding-3-small")
  {
    var encoding = GptEncoding.GetEncodingForModel(model);
    return encoding.Encode(text).Count;
  }
}