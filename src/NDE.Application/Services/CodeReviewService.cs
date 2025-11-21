using System.Text;

using Mapster;

using Microsoft.Extensions.Logging;

using NDE.Application.Integrations.Azure;
using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.ViewModels.Response.CodeReviews;
using NDE.Application.ViewModels.Response.LLMService;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;
using NDE.VectorStore.Extensions;
using NDE.VectorStore.Interfaces;
using NDE.VectorStore.Models;

namespace NDE.Application.Services;

public class CodeReviewService : BaseService, ICodeReviewService
{
  private readonly ILLMService _llmService;
  private readonly IPromptService _promptService;
  private readonly IEmbeddingService _embeddingService;
  private readonly IVectorStore _vectorStore;
  private readonly ICodeReviewRepository _codeReviewRepository;
  private readonly IAzureIntegration _azureIntegration;
  private readonly IPullRequestService _pullRequestService;
  private readonly IRepositoryService _repositoryService;
  private readonly IProjectService _projectService;
  private readonly IAuthTokenService _authTokenService;
  private readonly ILogger<CodeReviewService> _logger;
  private const string VectorCollection = "CodeReview";

  public CodeReviewService(
    ILLMService llmService,
    IPromptService promptService,
    IEmbeddingService embeddingService,
    IVectorStore vectorStore,
    ICodeReviewRepository codeReviewRepository,
    IPullRequestService pullRequestService,
    IRepositoryService repositoryService,
    IProjectService projectService,
    IAuthTokenService authTokenService,
    IAzureIntegration azureIntegration,
    ILogger<CodeReviewService> logger,
    INotificator notificator) : base(notificator)
  {
    _llmService = llmService;
    _promptService = promptService;
    _embeddingService = embeddingService;
    _vectorStore = vectorStore;
    _codeReviewRepository = codeReviewRepository;
    _pullRequestService = pullRequestService;
    _repositoryService = repositoryService;
    _projectService = projectService;
    _authTokenService = authTokenService;
    _azureIntegration = azureIntegration;
    _logger = logger;
  }

  public async Task<bool> ReviewAsync(StartReviewRequestViewModel request)
  {
    _logger.LogInformation("Iniciando processo de revisão");

    if (request is null)
    {
      _logger.LogWarning("Request nulo");
      Notify("Você precisa informar o diff que será validado.");
      return false;
    }

    if (request.Entries == null || !request.Entries.Any())
    {
      _logger.LogWarning("Nenhuma entry encontrada");
      Notify("Você precisa informar o diff que será validado.");
      return false;
    }

    _logger.LogInformation("Obtendo o projeto.");
    var project = await _projectService.GetByIdAsync(request.ProjectId);
    if (project == null)
    {
      Notify("Não foi possível localizar o projeto com o ID informado.");
      return false;
    }

    _logger.LogInformation("Obtendo o repositório");
    var repository = await _repositoryService.GetByIdAsync(request.RepositoryId);
    if (repository == null)
    {
      Notify("Não foi possível localizar o repositório com o ID informado.");
      return false;
    }

    _logger.LogInformation("Garantindo pull request");
    var pullRequest = await EnsurePullRequest(repositoryId: repository.Id, pullRequestId: request.PullRequestId, title: request.Title, description: request.Description);
    if (pullRequest == null) return false;

    var changeFiles = request.Entries
      .Select(entry => entry.FilePath)
      .Where(x => !string.IsNullOrWhiteSpace(x))
      .ToList();

    var codeReviewResponse = new List<LLMOutputResponseViewModel>();

    foreach (var entry in request.Entries)
    {
      var filePath = entry.FilePath;
      if (string.IsNullOrWhiteSpace(filePath))
      {
        _logger.LogWarning("Entry com FilePath vazio ignorado");
        continue;
      }

      _logger.LogInformation("Processando arquivo {File}", filePath);

      var existingCodeReview = await _codeReviewRepository.GetCodeReviewAsync(pullRequestId: pullRequest.Id, filePath: filePath);

      if (existingCodeReview != null)
      {
        _logger.LogInformation("Review existente encontrado para {File}", filePath);

        var diffChanged = existingCodeReview.CompareDiff(entry.Diff);

        Verdict currentVerdict = existingCodeReview.VerdictId;

        if (!diffChanged &&
            currentVerdict.Id != Verdict.Reproved.Id &&
            (currentVerdict.Id == Verdict.Pending.Id || currentVerdict.Id == Verdict.NoFeedback.Id))
        {
          _logger.LogInformation("Reutilizando sugestão pré-existente para {File}", filePath);
          codeReviewResponse.Add(new LLMOutputResponseViewModel
          {
            FilePath = filePath,
            Suggestion = existingCodeReview.Suggestion,
            FileLink = existingCodeReview.FileLink
          });
          continue;
        }

        await HandleExistingCodeReview(existingCodeReview, pullRequest, repository, project);

        if (currentVerdict.Id == Verdict.Approved.Id && !diffChanged)
        {
          _logger.LogInformation("Arquivo aprovado e sem mudanças: {File}", filePath);
          codeReviewResponse.Add(new LLMOutputResponseViewModel
          {
            FilePath = filePath,
            Suggestion = existingCodeReview.Suggestion,
            FileLink = existingCodeReview.FileLink
          });
          continue;
        }
      }

      _logger.LogInformation("Criando novo CodeReview para {File}", filePath);

      var newCodeReview = new CodeReview(
        pullRequestId: pullRequest.Id,
        filePath: entry.FilePath,
        diff: entry.Diff,
        language: entry.Language
      );

      var previousSuggestions = new Dictionary<string, string>();

      if (entry.Modifications != null && entry.Modifications.Count > 0)
      {
        foreach (var mod in entry.Modifications)
        {
          _logger.LogInformation("Processando modificação para {File}", filePath);
          var modification = new Modification(newCodeReview.Id, mod.CodeBlock);

          newCodeReview.AddModifications(modification);

          var similar = await GetSimilarSuggestion(
            pullRequest.PullRequestId,
            repository,
            project,
            modification
          );

          if (similar == null)
          {
            _logger.LogInformation("Nenhuma sugestão similar encontrada");
            continue;
          }

          foreach (var kvp in similar)
            if (!previousSuggestions.ContainsKey(kvp.Key))
              previousSuggestions[kvp.Key] = kvp.Value;
        }
      }

      newCodeReview.SetFileLink(pullRequestId: pullRequest.PullRequestId, repositoryUrl: repository.Url, filePath: entry.FilePath);

      var previousSuggestionsContext = BuildSimilarReviewContextMarkdown(previousSuggestions);

      _logger.LogInformation("Montando prompt para LLM");
      var instructions = await _promptService.BuildInstructions(changeFiles, previousSuggestionsContext, request.SystemPrompt);

      _logger.LogInformation("Chamando LLM");
      //var suggestion = await _llmService.SendMessage(instructions, entry.Prompt);

      var suggestion = await LLMDebugRandom();

      var isNoFeedback = suggestion.Text.Contains("Sem feedback.", StringComparison.OrdinalIgnoreCase);
      if (isNoFeedback)
      {
        _logger.LogInformation("LLM retornou Sem feedback.");
        newCodeReview.SetClosed();
      }

      newCodeReview.SetSuggestion(suggestion: suggestion.Text, tokens: suggestion.Tokens);

      pullRequest.SetTokens(suggestion.Tokens);

      await _codeReviewRepository.Add(newCodeReview);

      _logger.LogInformation("Salvando alterações no banco para {File}", filePath);

      if (!isNoFeedback)
      {
        codeReviewResponse.Add(new LLMOutputResponseViewModel
        {
          FilePath = filePath,
          Suggestion = newCodeReview.Suggestion,
          FileLink = newCodeReview.FileLink
        });
      }
    }

    var saved = await SaveCodeReviewChanges(pullRequest.PullRequestId, repository.Id);
    if (!saved)
      return false;

    var result = await _pullRequestService.UpdateAsync(pullRequest);

    if (result < 0)
      _logger.LogCritical("Houve um erro ao tentar atualizar o Pull Request, porém a revisão foi finalizada.");


    var portalUrl = await _authTokenService.GenerateAccessUrlAsync(
      new Dictionary<string, string>
      {
        { "prId", pullRequest.Id.ToString() },
        { "projectId", project.Id.ToString() },
        { "reposId", repository.Id.ToString() }
      });

    await _azureIntegration.AddCodeReviewToPR(
      collectionUrl: project.CollectionUrl,
      repositoryId: repository.Id,
      projectName: project.Name,
      portalUrl: portalUrl,
      repositoryUrl: repository.Url,
      token: request.AuthToken,
      pullRequestId: pullRequest.PullRequestId,
      reviews: codeReviewResponse
    );

    _logger.LogInformation("Processo de revisão concluído");

    return true;
  }

  private string BuildSimilarReviewContextMarkdown(Dictionary<string, string> suggestions)
  {
    _logger.LogInformation("Construindo contexto de sugestões anteriores");

    if (suggestions == null || suggestions.Count == 0)
    {
      _logger.LogInformation("Nenhuma sugestão anterior encontrada");
      return string.Empty;
    }

    var markdown = new StringBuilder();

    markdown.AppendLine("## Exemplos de Revisões Anteriores");
    markdown.AppendLine("**Aprenda com estes exemplos:**");
    markdown.AppendLine("- Exemplos aprovados mostram o padrão de qualidade esperado");
    markdown.AppendLine("- Exemplos não aprovados mostram o que evitar");
    markdown.AppendLine("### Use estes exemplos para calibrar suas próprias revisões:");
    markdown.AppendLine();

    var approved = suggestions
      .Where(s => s.Value.Equals("Aprovado", StringComparison.OrdinalIgnoreCase))
      .Select(s => s.Key)
      .Take(3)
      .ToList();

    var reproved = suggestions
      .Where(s => s.Value.Equals("Reprovado", StringComparison.OrdinalIgnoreCase))
      .Select(s => s.Key)
      .Take(3)
      .ToList();

    if (approved.Count > 0)
    {
      _logger.LogInformation("Incluindo sugestões aprovadas ({Count})", approved.Count);
      markdown.AppendLine("#### Sugestões Aprovadas");
      markdown.AppendLine("- **Ação:** Priorize sugestões com este estilo/abordagem");
      foreach (var suggestion in approved)
        markdown.AppendLine($"- {suggestion}");
      markdown.AppendLine();
    }

    if (reproved.Count > 0)
    {
      _logger.LogInformation("Incluindo sugestões reprovadas ({Count})", reproved.Count);
      markdown.AppendLine("#### Sugestões Rejeitadas");
      markdown.AppendLine("- **Ação:** Evite sugestões similares");
      foreach (var suggestion in reproved)
        markdown.AppendLine($"- {suggestion}");
      markdown.AppendLine();
    }

    return markdown.ToString();
  }

  private async Task<PullRequest?> EnsurePullRequest(Guid repositoryId, int pullRequestId, string title, string description)
  {
    _logger.LogInformation("Verificando PR {Id}", pullRequestId);
    var pullRequest = await _pullRequestService.GetAsync(repositoryId: repositoryId, pullRequestId: pullRequestId);

    if (pullRequest is null)
    {
      _logger.LogInformation("Criando novo PR {Id}", pullRequestId);
      pullRequest = new PullRequest(pullRequestId: pullRequestId, repositoryId: repositoryId, title: title, description: description);

      var result = await _pullRequestService.CreateAsync(pullRequest);
      if (result == null)
      {
        _logger.LogError("Falha ao criar PR");
        return null;
      }
    }

    return pullRequest;
  }

  private async Task HandleExistingCodeReview(CodeReview codeReview, PullRequest pullRequest, Repository repository, Project project)
  {
    _logger.LogInformation("Atualizando estado de CodeReview existente {Id}", codeReview.Id);

    if (codeReview.VerdictId == Verdict.Pending.Id)
    {
      codeReview.SetVerdict(Verdict.Abandoned.Id);
      codeReview.SetClosed();
      _codeReviewRepository.Update(codeReview);
    }
    else if (codeReview.VerdictId == Verdict.Reproved.Id)
    {
      codeReview.SetClosed();
      _codeReviewRepository.Update(codeReview);
    }
    else if (!codeReview.Closed &&
             (codeReview.VerdictId == Verdict.Approved.Id ||
              codeReview.VerdictId == Verdict.Partial.Id))
    {
      codeReview.SetClosed();
      _codeReviewRepository.Update(codeReview);
    }

    _logger.LogInformation("Persistindo vetores anteriores");
    await SaveVectorStore(
      repositoryId: repository.Id,
      projectId: project.Id,
      pullRequestId: pullRequest.PullRequestId,
      codeReviewId: codeReview.Id,
      verdictId: codeReview.VerdictId,
      feedback: codeReview.Feedback,
      filePath: codeReview.FilePath,
      suggestion: codeReview.Suggestion,
      modifications: codeReview.Modifications);
  }

  private async Task<bool> SaveCodeReviewChanges(int pullRequestId, Guid repositoryId)
  {
    _logger.LogInformation("Salvando alterações do CodeReview");

    if (!Commit(await _codeReviewRepository.SaveChangesAsync()))
    {
      _logger.LogError("Falha ao salvar alterações no CodeReview");
      Notify("Ocorreu um erro ao salvar as alterações.");
      return false;
    }

    return true;
  }

  private async Task<bool> SaveVectorStore(Guid repositoryId, Guid projectId, Guid codeReviewId, int pullRequestId, int verdictId, string feedback, string filePath, string suggestion, List<Modification> modifications)
  {
    _logger.LogInformation("Persistindo embeddings no vector store para CodeReview {CodeReviewId}", codeReviewId);

    if (modifications == null || modifications.Count == 0)
    {
      _logger.LogInformation("Nenhuma modification para persistir no vector store");
      return true;
    }

    Verdict verdict = verdictId;

    foreach (var mod in modifications)
    {
      if (mod.Vector == null || mod.Vector.Length == 0)
      {
        _logger.LogInformation("Modification {ModId} sem vetor, ignorando", mod.Id);
        continue;
      }

      _logger.LogInformation("Processando Modification {ModId} para VectorStore", mod.Id);

      var data = new CodeReviewSuggestion(
        codeReviewId: codeReviewId,
        codeModificationId: mod.Id,
        repositoryId: repositoryId,
        projectId: projectId,
        pullRequestId: pullRequestId,
        filePath: filePath,
        codeBlock: mod.CodeBlock,
        suggestion: suggestion,
        feedback: feedback,
        verdict: verdict.Description,
        weight: verdict.Id,
        createdAt: DateTime.UtcNow
      );

      await _vectorStore.UpsertAsync(VectorCollection, mod.Id, mod.Vector, data);

      _logger.LogInformation("VectorStore atualizado para Modification {ModId}", mod.Id);
    }

    return true;
  }

  public async Task<bool> UpdateCodeReviewAsync(UpdateCodeReviewRequestViewModel request)
  {
    var codeReview = await _codeReviewRepository.GetCodeReviewAsync(request.Id);
    if (codeReview == null)
    {
      _logger.LogWarning("CodeReview {CodeReviewId} não encontrado ao tentar atualizar.", request.Id);
      Notify("O Code Review informado não foi encontrado.");
      return false;
    }

    codeReview.SetVerdict(request.VerdictId);
    codeReview.SetFeedback(request.Feedback);
    codeReview.SetClosed();

    _codeReviewRepository.Update(codeReview);

    return Commit(await _codeReviewRepository.SaveChangesAsync());
  }

  private async Task<Dictionary<string, string>?> GetSimilarSuggestion(int pullRequestId, Repository repository, Project project, Modification modification)
  {
    _logger.LogInformation("Consultando vector store para sugestões similares");

    var embedding = modification.Vector;

    if (embedding == null || embedding.Length == 0)
    {
      _logger.LogInformation("Embedding não encontrado. Gerando novo.");
      embedding = await _embeddingService.GenerateEmbedding(modification.CodeBlock);
      if (embedding == null || embedding.Length == 0)
        return null;
      
      modification.SetVector(embedding);
    }

    var filters = new Dictionary<string, string>
    {
      { "repositoryId", repository.Id.ToString() }
    };

    var results = await _vectorStore.SmartSearchAsync<CodeReviewSuggestion>(
      VectorCollection,
      embedding,
      filters,
      6,
      0.75f
    );

    var candidates = results?
      .Where(r => !string.IsNullOrWhiteSpace(r.Payload?.Suggestion))
      .Select(r => new { Suggestion = r.Payload!.Suggestion.Trim(), Verdict = r.Payload!.Verdict?.Trim() ?? string.Empty })
      .ToList();

    if (candidates == null || candidates.Count == 0)
    {
      _logger.LogInformation("Nenhum candidato encontrado");
      return null;
    }

    var best = new Dictionary<string, string>();

    foreach (var c in candidates.Where(c => c.Verdict.Equals("Aprovado", StringComparison.OrdinalIgnoreCase)))
      best[c.Suggestion] = c.Verdict;

    foreach (var c in candidates.Where(c => c.Verdict.Equals("Reprovado", StringComparison.OrdinalIgnoreCase)))
      best[c.Suggestion] = c.Verdict;

    if (best.Count == 0)
    {
      _logger.LogInformation("Nenhum item elegível pós-filtragem");
      return null;
    }

    AppMetrics.ObserveSimilarFound(
      pullRequestId,
      project.Name,
      repository.Name,
      best.Keys.ToList(),
      best.Count
    );

    return best;
  }

  public async Task<PullRequestResponseViewModel?> GetAllCodeReviewAsync(Guid repositoryId, Guid projectId, Guid pullRequestId)
  {
    var project = await _projectService.GetByIdAsync(projectId);
    if (project == null)
    {
      _logger.LogWarning("Projeto {ProjectId} não encontrado ao buscar relatório do PR {PullRequestId}.",
        projectId, pullRequestId);

      Notify("O projeto não foi encontrado.");
      return null;
    }

    var repository = await _repositoryService.GetByIdAsync(repositoryId);
    if (repository == null)
    {
      _logger.LogWarning("Repositório {RepositoryId} não encontrado para o projeto {ProjectId}.",
        repositoryId, project.Id);

      Notify("O repositório não foi encontrado.");
      return null;
    }

    if (repository.ProjectId != project.Id)
    {
      _logger.LogError(
        "Inconsistência: Repositório {RepositoryId} pertence ao projeto {RepositoryProjectId}, não ao projeto {ProjectId}.",
        repositoryId, repository.ProjectId, project.Id);

      Notify("Repositório não pertence ao projeto informado.");
      return null;
    }

    var pullRequest = await _pullRequestService.GetAsync(
      id: pullRequestId,
      repositoryId: repository.Id,
      includeCodeReviews: true);

    if (pullRequest == null)
    {
      _logger.LogWarning("Pull Request {PullRequestId} não encontrado no repositório {RepositoryId}.",
        pullRequestId, repository.Id);

      Notify("O Pull Request não foi encontrado.");
      return null;
    }

    return pullRequest.Adapt<PullRequestResponseViewModel>();
  }

  private async Task<List<LLMResponse>> LLMDebug()
  {
    var output = new List<LLMResponse>
  {
    new LLMResponse
    {
      Text =
        "Arquivo: src/NDE.Application/Services/CodeReviewService.cs\n" +
        "Severidade: Alta\n" +
        "Resumo: O método de análise não trata cenários de diff vazio ou nulo.\n\n" +
        "Sugestão:\n" +
        "- Validar explicitamente se o diff está vazio antes de prosseguir.\n" +
        "- Retornar um resultado neutro ou pular o arquivo para evitar exceções desnecessárias.\n" +
        "- Registrar em log os casos ignorados para facilitar troubleshooting.",
      Tokens = 120
    },
    new LLMResponse
    {
      Text =
        "Arquivo: src/NDE.Domain/Entities/CodeReviews/CodeReview.cs\n" +
        "Severidade: Média\n" +
        "Resumo: A lógica de geração de hash está duplicada entre GenerateHash e CompareDiff.\n\n" +
        "Sugestão:\n" +
        "- Centralizar a normalização e o cálculo do hash em um único método interno.\n" +
        "- Reduzir branches condicionais para deixar a leitura mais direta.\n" +
        "- Facilitar a criação de testes unitários reutilizando a mesma função de hash.",
      Tokens = 110
    },
    new LLMResponse
    {
      Text =
        "Arquivo: src/NDE.Data/Mappings/CodeReviewConfiguration.cs\n" +
        "Severidade: Baixa\n" +
        "Resumo: Campos textuais obrigatórios podem gerar registros com string vazia sem validação de domínio.\n\n" +
        "Sugestão:\n" +
        "- Garantir validação de domínio no aggregate root antes de persistir (Title, Suggestion, Feedback).\n" +
        "- Opcionalmente, adicionar constraints de tamanho mínimo se fizer sentido para o negócio.\n" +
        "- Manter consistência entre validações de domínio e anotações de validação na API.",
      Tokens = 115
    }
  };

    return await Task.FromResult(output);
  }

  private async Task<LLMResponse> LLMDebugRandom()
  {
    var outputs = await LLMDebug();

    if (outputs == null || outputs.Count == 0)
      return new LLMResponse { Text = string.Empty, Tokens = 0 };

    var index = Random.Shared.Next(outputs.Count);
    return outputs[index];
  }
}
