using System.ClientModel;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Response.LLMService;
using NDE.Domain.Utils;
using NDE.VectorStore.Options;

using OpenAI;
using OpenAI.Chat;

using TiktokenSharp;

namespace NDE.Application.Services;

public class LLMService : ILLMService
{
  private readonly AppSettings _settings;
  private readonly ILogger<LLMService> _logger;
  private readonly ChatClient _client;
  private readonly TikToken? _tokenEncoder;
  private int _tokensConsumed;

  public LLMService(IOptions<AppSettings> settings, IOptions<VectorStoreOptions> vectorStore, ILogger<LLMService> logger)
  {
    _settings = settings.Value;
    _logger = logger;

    _logger.LogInformation("Inicializando LLMService com modelo {Model}", _settings.LLMService.ModelName);

    _client = new ChatClient(
      model: _settings.LLMService.ModelName,
      credential: new ApiKeyCredential(_settings.LLMService.ApiKey),
      options: new OpenAIClientOptions
      {
        Endpoint = new Uri(_settings.LLMService.Endpoint)
      });

    try
    {
      _tokenEncoder = TikToken.EncodingForModel(_settings.LLMService.ModelName);

      if (_tokenEncoder == null)
        _logger.LogWarning("Falha ao carregar encoder de tokens para o modelo {Model}", _settings.LLMService.ModelName);
      else
        _logger.LogInformation("Encoder carregado com sucesso");
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Falha ao inicializar encoder de tokens para o modelo {Model}", _settings.LLMService.ModelName);
      _tokenEncoder = null;
    }
  }

  private ChatMessage[] CreateMessages(string instructions, string content)
  {
    _logger.LogDebug("Criando mensagens do prompt");

    if (string.IsNullOrWhiteSpace(instructions))
      throw new ArgumentException("Instruções não podem estar vazias", nameof(instructions));

    if (string.IsNullOrWhiteSpace(content))
      throw new ArgumentException("Conteúdo não pode estar vazio", nameof(content));

    _logger.LogDebug("Tamanho do system prompt: {len} chars", instructions.Length);
    _logger.LogDebug("Tamanho do user prompt: {len} chars", content.Length);

    return
    [
      new SystemChatMessage(instructions),
      new UserChatMessage(content)
    ];
  }

  private int EstimateTokens(string instructions, string content)
  {
    if (_tokenEncoder == null)
    {
      _logger.LogWarning("Token encoder não disponível. Estimativa será zero.");
      return 0;
    }

    _logger.LogDebug("Estimando tokens...");

    var sysTokens = _tokenEncoder.Encode(instructions ?? string.Empty).Count;
    var userTokens = _tokenEncoder.Encode(content ?? string.Empty).Count;

    var total = sysTokens + userTokens;
    var buffer = (int)Math.Ceiling(total * 0.1);

    var estimated = total + buffer;

    _logger.LogInformation(
      "Estimativa de tokens → System: {sys}, User: {user}, Total: {total}, Buffer(+10%): {buffer}, Final: {final}",
      sysTokens, userTokens, total, buffer, estimated
    );

    return estimated;
  }

  public Task<LLMResponse> SendMessage(string instructions, string content)
  {
    return SendMessage(instructions, content, CancellationToken.None);
  }

  public async Task<LLMResponse> SendMessage(string instructions, string content, CancellationToken ct)
  {
    _logger.LogInformation("Iniciando chamada ao modelo {Model}", _settings.LLMService.ModelName);

    try
    {
      var estimatedTokens = EstimateTokens(instructions, content);
      _logger.LogInformation("Tokens estimados para esta requisição: {Tokens}", estimatedTokens);

      var messages = CreateMessages(instructions, content);

      _logger.LogInformation(
        "Chamando OpenAI: System={sysChars} chars, User={userChars} chars",
        instructions.Length,
        content.Length
      );

      ClientResult<ChatCompletion> completionResult = await _client.CompleteChatAsync(messages, cancellationToken: ct);
      ChatCompletion completion = completionResult.Value;

      if (completion.Content == null || completion.Content.Count == 0)
      {
        _logger.LogError("Resposta da OpenAI não contém conteúdo");
        throw new InvalidOperationException("Nenhuma resposta retornada pela OpenAI");
      }

      var usage = completion.Usage;
      var tokens = usage?.TotalTokenCount ?? 0;

      _logger.LogInformation("Tokens usados pela OpenAI → Total: {total}", tokens);

      if (usage is not null)
      {
        Interlocked.Add(ref _tokensConsumed, tokens);
        _logger.LogInformation("Total acumulado de tokens consumidos: {Value}", _tokensConsumed);
      }

      var text = string.Concat(completion.Content.Select(c => c.Text));

      if (string.IsNullOrWhiteSpace(text))
        _logger.LogWarning("Resposta da OpenAI está vazia");

      _logger.LogDebug("Resposta recebida com {Len} chars", text.Length);

      return new LLMResponse
      {
        Text = text,
        Tokens = tokens
      };
    }
    catch (OperationCanceledException)
    {
      _logger.LogWarning("Chamada ao modelo {Model} cancelada", _settings.LLMService.ModelName);
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Erro ao comunicar com OpenAI (Endpoint={Endpoint}, Model={Model})",
        _settings.LLMService.Endpoint,
        _settings.LLMService.ModelName
      );

      throw new InvalidOperationException("Falha na comunicação com OpenAI", ex);
    }
  }

  public int GetTokensConsumed()
  {
    var value = Volatile.Read(ref _tokensConsumed);
    _logger.LogDebug("Tokens consumidos acumulados solicitados ({Tokens})", value);
    return value;
  }
}
