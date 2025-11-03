using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NDE.Application.Helpers;
using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.ViewModels.Response.CodeReviews;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Domain.Utils;
using NDE.Observability.Metrics;
using NDE.VectorStore.Extensions;
using NDE.VectorStore.Interfaces;
using NDE.VectorStore.Models;

namespace NDE.Application.Services;

public class AzurePullRequestService : BaseService, IAzurePullRequestService
{
  private readonly IEmbeddingService _embeddingService;
  private readonly AppSettings _settings;
  private readonly IVectorStore _vectorStore;
  private readonly ICodeReviewRepository _codeReviewRepository;
  private readonly IAzurePullRequestRepository _azurePullRequestRepository;
  private readonly IAzureRepositoryRepository _azureRepositoryRepository;
  private readonly IAzureProjectRepository _azureProjectRepository;
  private readonly ILogger<AzurePullRequestService> _logger;
  private const string VectorCollection = "CodeReview";

  public AzurePullRequestService(
      IOptions<AppSettings> settings,
      IEmbeddingService embeddingService,
      IAzurePullRequestRepository pullRequestRepository,
      IVectorStore indexStore,
      IAzureRepositoryRepository azureRepositoryRepository,
      ICodeReviewRepository codeReviewRepository,
      IAzureProjectRepository azureProjectRepository,
      ILogger<AzurePullRequestService> logger,
      INotificator notificator) : base(notificator)
  {
    _settings = settings.Value;
    _embeddingService = embeddingService;
    _azurePullRequestRepository = pullRequestRepository;
    _vectorStore = indexStore;
    _azureRepositoryRepository = azureRepositoryRepository;
    _azureProjectRepository = azureProjectRepository;
    _codeReviewRepository = codeReviewRepository;
    _logger = logger;
  }

  public async Task<RequestResponseViewModel?> ReviewRequestAsync(ReviewRequestViewModel reviewRequest)
  {
    if (reviewRequest is null)
    {
      _logger.LogWarning("Você precisa informar o diff que será validado.");
      Notify("Você precisa informar o diff que será validado.");
      return null;
    }

    var repository = await _azureRepositoryRepository.GetById(reviewRequest.RepositoryId);
    if (repository is null)
    {
      _logger.LogWarning("Não foi localizado o repositório com o Id informado.");
      Notify("Não foi localizado o repositório com o Id informado.");
      return null;
    }

    var project = await _azureProjectRepository.GetById(repository.ProjectId);
    if (project is null)
    {
      _logger.LogWarning("Não foi localizado o projeto com o Id informado.");
      Notify("Não foi localizado o projeto com o Id informado.");
      return null;
    }

    var pullRequest = await _azurePullRequestRepository.GetPullRequestAsync(
        repositoryId: repository.Id,
        pullRequestId: reviewRequest.PullRequestId
    );

    if (pullRequest is null)
    {
      pullRequest = new AzurePullRequest(
          id: reviewRequest.PullRequestId,
          repositoryId: repository.Id
      );

      await _azurePullRequestRepository.Add(pullRequest);
      if (!await SavePullRequestChanges(pullRequest.Id, repository.Id))
        return null;
    }

    var filePath = reviewRequest.FilePath ?? string.Empty;
    var normalizedDiff = NormalizeText(reviewRequest.Diff);
    var newHash = ComputeSha512Hex(normalizedDiff);

    var existingCodeReview = await _codeReviewRepository.GetCodeReviewAsync(pullRequestId: pullRequest.Id, filePath: filePath, closed: false);

    if (existingCodeReview != null)
    {
      bool diffChanged = string.IsNullOrEmpty(existingCodeReview.DiffHash) ||
                         !string.Equals(existingCodeReview.DiffHash, newHash, StringComparison.OrdinalIgnoreCase);

      Verdict currentVerdict = existingCodeReview.VerdictId;

      if (!diffChanged && currentVerdict.Id != Verdict.Reproved.Id && currentVerdict.Id != Verdict.Pending.Id)
      {
        return new RequestResponseViewModel
        {
          Fingerprint = existingCodeReview.Fingerprint,
          Suggestion = existingCodeReview.Suggestion,
          TokensConsumed = existingCodeReview.TokensConsumed,
          FromCache = true
        };
      }

      await HandleExistingCodeReview(existingCodeReview, pullRequest, repository, project);
    }

    var newCodeReview = new CodeReview(
        pullRequestId: pullRequest.Id,
        filePath: filePath,
        diff: normalizedDiff
    );

    newCodeReview.SetLanguage(reviewRequest.Language);
    newCodeReview.SetVerdict(Verdict.Pending.Id);
    newCodeReview.SetDiffHash(newHash);

    await _codeReviewRepository.Add(newCodeReview);

    if (_settings.UseVectorStore)
    {
      await GetSimilarSuggestion(
          pullRequest: pullRequest,
          repository: repository,
          project: project,
          codeReview: newCodeReview);
    }

    await SaveCodeReviewChanges(pullRequest.Id, repository.Id);

    return new RequestResponseViewModel
    {
      Fingerprint = newCodeReview.Fingerprint,
      Suggestion = newCodeReview.Suggestion,
      TokensConsumed = newCodeReview.TokensConsumed,
      FromCache = false
    };
  }

  public async Task<bool> SaveReviewAsync(SaveReviewRequestViewModel suggestions)
  {
    if (!suggestions.Suggestions.Any())
    {
      Notify("Você precisa informar uma ou mais sugestões.");
      return false;
    }

    var repository = await _azureRepositoryRepository.GetRepositoryByIdAsync(suggestions.RepositoryId);
    if (repository is null)
    {
      _logger.LogWarning("Não foi localizado o repositório com o Id informado.");
      return false;
    }

    var project = await _azureProjectRepository.GetById(repository.ProjectId);
    if (project is null)
    {
      _logger.LogWarning("Não foi localizado o projeto com o Id informado.");
      return false;
    }

    var pullRequest = await _azurePullRequestRepository.GetPullRequestAsync(
        repositoryId: repository.Id,
        pullRequestId: suggestions.PullRequestId);
    if (pullRequest is null)
    {
      _logger.LogWarning("Não foi localizado o pull request com o Id informado.");
      return false;
    }

    bool hasChanges = false;

    foreach (var suggestion in suggestions.Suggestions)
    {
      var codeReview = await _codeReviewRepository.GetCodeReviewAsync(pullRequestId: pullRequest.Id, fingerprint: suggestion.Fingerprint);

      if (codeReview == null)
      {
        Notify($"Não foi encontrado nenhum code review com o fingerprint {suggestion.Fingerprint}");
        continue;
      }

      if (codeReview.Closed)
        continue;

      bool isNoFeedback = suggestion.Suggestion?.Contains("Sem feedback.") ?? false;
      bool hasExistingFeedback = !string.IsNullOrWhiteSpace(codeReview.Suggestion) &&
                                 !codeReview.Suggestion.Contains("Sem feedback.");

      if (!isNoFeedback || (isNoFeedback && !hasExistingFeedback))
      {
        if (isNoFeedback)
          codeReview.SetClosed();

        codeReview.SetTokens(suggestion.TokensConsumed);
        codeReview.SetSuggestion(suggestion.Suggestion);

        _codeReviewRepository.Update(codeReview);

        if (!isNoFeedback && _settings.UseVectorStore)
        {
          await SaveOrUpdateVectorStore(
              pullRequest: pullRequest,
              repository: repository,
              project: project,
              codeReview: codeReview);
        }

        hasChanges = true;
      }
    }

    if (hasChanges)
      return Commit(await _codeReviewRepository.SaveChangesAsync());

    return true;
  }

  public async Task<bool> HandleReviewAsync(int pullRequestId, Guid repositoryId, string threadUrl, string comment)
  {
    if (string.IsNullOrWhiteSpace(comment))
    {
      _logger.LogWarning("O conteúdo do comentário está vázio.");
      return false;
    }

    var repository = await _azureRepositoryRepository.GetRepositoryByIdAsync(repositoryId);
    if (repository is null)
    {
      _logger.LogWarning("Não foi localizado o repositório com o Id informado.");
      return false;
    }

    var project = await _azureProjectRepository.GetById(repository.ProjectId);
    if (project is null)
    {
      _logger.LogWarning("Não foi localizado o projeto com o Id informado.");
      return false;
    }

    var pullRequest = await _azurePullRequestRepository.GetPullRequestAsync(
        repositoryId: repository.Id,
        pullRequestId: pullRequestId,
        includeChildren: true);
    if (pullRequest is null)
    {
      _logger.LogWarning("Não foi localizado o pull request com o Id informado.");
      return false;
    }

    if (pullRequest.CodeReviews == null || pullRequest.CodeReviews.Count == 0)
    {
      _logger.LogWarning("Não foi encontrado code review para o pull request informado.");
      return false;
    }

    bool hasChanges = false;

    foreach (var codeReview in pullRequest.CodeReviews)
    {
      if (string.IsNullOrWhiteSpace(codeReview.Suggestion))
        continue;

      int verdictId = ReviewFeedbackParser.IsAcceptedByHash(content: comment, hash: codeReview.Fingerprint);

      if (verdictId == -1)
      {
        _logger.LogWarning("Houve um erro ao tentar obter o feedback do code review: {Fingerprint}", codeReview.Fingerprint);
        continue;
      }

      if (codeReview.VerdictId == verdictId)
        continue;

      codeReview.SetClosed(verdictId == Verdict.Approved.Id);

      var reason = ReviewFeedbackParser.GetReason(content: comment, hash: codeReview.Fingerprint);
      if (!string.IsNullOrWhiteSpace(reason))
        codeReview.SetFeedback(reason);

      codeReview.SetVerdict(verdictId);

      if (_settings.UseVectorStore)
      {
        await SaveOrUpdateVectorStore(
            pullRequest: pullRequest,
            repository: repository,
            project: project,
            codeReview: codeReview);
      }

      _codeReviewRepository.Update(codeReview);
      hasChanges = true;
    }

    if (hasChanges)
      return Commit(await _codeReviewRepository.SaveChangesAsync());

    return true;
  }

  public async Task<PreviousSuggestionsResponseViewModel?> GetPreviousSuggestions(
    Guid repositoryId, 
    int pullRequestId)
{
    var approved = await _codeReviewRepository.GetSummaryReviewApproved(
        pullRequestId: pullRequestId, 
        repositoryId: repositoryId);

    var reproved = await _codeReviewRepository.GetSummaryReviewReproved(
        pullRequestId: pullRequestId,
        repositoryId: repositoryId);

    if (approved == null && reproved == null)
      return null;

    var context = BuildContextMarkdown(approved: approved, reproved: reproved);

    return new PreviousSuggestionsResponseViewModel
    {
        PreviusSuggestions = context
    };
}

  private string BuildContextMarkdown(CodeReview? approved, CodeReview? reproved)
  {
    var markdown = new StringBuilder();

    markdown.AppendLine("## Exemplos de Revisões Anteriores");
    markdown.AppendLine("**Aprenda com estes exemplos:**");
    markdown.AppendLine("- Exemplos aprovados mostram o padrão de qualidade esperado");
    markdown.AppendLine("- Exemplos não aprovados mostram o que evitar");
    markdown.AppendLine("### Use estes exemplos para calibrar suas próprias revisões:");
    markdown.AppendLine();

    if (approved != null)
    {
      markdown.AppendLine("#### Sugestão Aprovada");
      markdown.AppendLine($"- **Arquivo:** `{approved.FilePath}`");
      markdown.AppendLine($"- **Conteúdo:** {approved.SummaryReview}");
      markdown.AppendLine($"**Data:** {approved.CreatedAt:dd/MM/yyyy}");
      markdown.AppendLine($"- **Ação:** Priorize sugestões com este estilo/abordagem");
      markdown.AppendLine();
    }

    if (reproved != null)
    {
      markdown.AppendLine("#### Sugestão Rejeitada");
      markdown.AppendLine($"- **Arquivo:** `{reproved.FilePath}`");
      markdown.AppendLine($"- **Conteúdo:** {reproved.SummaryReview}");
      markdown.AppendLine($"**Data:** {reproved.CreatedAt:dd/MM/yyyy}");
      markdown.AppendLine($"- **Ação:** Evite sugestões similares");
      markdown.AppendLine();
    }

    return markdown.ToString();
  }

  private async Task HandleExistingCodeReview(
      CodeReview existingCodeReview,
      AzurePullRequest pullRequest,
      AzureRepository repository,
      AzureProject project)
  {
    if (existingCodeReview.VerdictId == Verdict.Pending.Id)
    {
      existingCodeReview.SetVerdict(Verdict.Abondoned.Id);
      existingCodeReview.SetClosed();
      _codeReviewRepository.Update(existingCodeReview);

      if (_settings.UseVectorStore)
        await SaveOrUpdateVectorStore(pullRequest, repository, project, existingCodeReview);
    }
    else if (existingCodeReview.VerdictId == Verdict.Reproved.Id)
    {
      existingCodeReview.SetClosed();
      _codeReviewRepository.Update(existingCodeReview);

      if (_settings.UseVectorStore)
        await SaveOrUpdateVectorStore(pullRequest, repository, project, existingCodeReview);
    }
  }

  private async Task<bool> GetSimilarSuggestion(
      AzurePullRequest pullRequest,
      AzureRepository repository,
      AzureProject project,
      CodeReview codeReview)
  {
    float[]? embedding = codeReview.Vector;

    if (embedding == null || embedding.Length == 0)
    {
      embedding = await _embeddingService.GenerateEmbedding(codeReview.Diff);
      if (embedding == null || embedding.Length == 0)
        return false;

      codeReview.SetVector(embedding);
    }

    var filters = new Dictionary<string, string>
        {
            { "repositoryId", repository.Id.ToString() }
        };

    var results = await _vectorStore.SmartSearchAsync<CodeReviewSuggestion>(
        collection: VectorCollection,
        query: embedding,
        filters: filters,
        topK: 5,
        minScore: 0.75f
    );

    var candidates = results?
        .Where(r =>
            !string.IsNullOrWhiteSpace(r.Payload?.Suggestion) &&
            r.Payload!.Fingerprint != codeReview.Fingerprint)
        .Select(r => new
        {
          Suggestion = r.Payload!.Suggestion!.Trim(),
          Verdict = r.Payload!.Verdict?.Trim() ?? string.Empty
        })
        .ToList();

    if (candidates == null || candidates.Count == 0)
      return false;

    var bestSuggestions = new Dictionary<string, string>();

    var approved = candidates.FirstOrDefault(c => c.Verdict.Equals("Aprovado", StringComparison.OrdinalIgnoreCase));
    if (approved != null)
      bestSuggestions[approved.Suggestion] = approved.Verdict;

    var rejected = candidates.FirstOrDefault(c => c.Verdict.Equals("Reprovado", StringComparison.OrdinalIgnoreCase));
    if (rejected != null)
      bestSuggestions[rejected.Suggestion] = rejected.Verdict;

    if (!bestSuggestions.Any())
      return false;

    AppMetrics.RecordSimilarFound(
        pullRequestId: pullRequest.Id,
        projectName: project.ProjectName,
        repositoryName: repository.RepositoryName,
        payloads: bestSuggestions.Keys.ToList(),
        quantity: bestSuggestions.Count
    );

    return true;
  }

  private async Task<bool> SaveOrUpdateVectorStore(
      AzurePullRequest pullRequest,
      AzureRepository repository,
      AzureProject project,
      CodeReview codeReview)
  {
    if (codeReview.Vector == null || codeReview.Vector.Length == 0)
    {
      _logger.LogWarning("Não foi possível salvar no vector store. Vector vazio para o CodeReview: {CodeReviewId}", codeReview.Id);
      return false;
    }

    Verdict verdict = codeReview.VerdictId;

    var suggestion = new CodeReviewSuggestion(
        codeReviewId: codeReview.Id,
        fingerprint: codeReview.Fingerprint,
        repositoryName: repository.RepositoryName,
        projectName: project.ProjectName,
        repositoryId: pullRequest.RepositoryId,
        projectId: project.Id,
        pullRequestId: pullRequest.Id,
        file: codeReview.FilePath,
        diff: codeReview.Diff,
        suggestion: codeReview.Suggestion,
        verdict: verdict.Description,
        feedback: codeReview.Feedback,
        weight: verdict.Id,
        createdAt: DateTime.UtcNow
    );

    await _vectorStore.UpsertAsync(VectorCollection, codeReview.Id, codeReview.Vector, suggestion);

    AppMetrics.RecordCodeReviewVectorCreated(
        pullRequestId: pullRequest.Id,
        repositoryName: repository.RepositoryName,
        projectName: project.ProjectName,
        verdict: verdict.Description
    );

    return true;
  }

  private async Task<bool> SaveCodeReviewChanges(int pullRequestId, Guid repositoryId)
  {
    if (!Commit(await _codeReviewRepository.SaveChangesAsync()))
    {
      _logger.LogError(
          "Falha ao salvar alterações no repositório de Code Reviews. PullRequestId: {PullRequestId}, RepositoryId: {RepositoryId}",
          pullRequestId,
          repositoryId
      );

      Notify("Ocorreu um erro ao salvar as alterações do Code Review. Tente novamente ou contate o administrador.");
      return false;
    }

    return true;
  }

  private async Task<bool> SavePullRequestChanges(int pullRequestId, Guid repositoryId)
  {
    if (!Commit(await _azurePullRequestRepository.SaveChangesAsync()))
    {
      _logger.LogError(
          "Falha ao salvar alterações no repositório de Pull Requests. PullRequestId: {PullRequestId}, RepositoryId: {RepositoryId}",
          pullRequestId,
          repositoryId
      );

      Notify("Ocorreu um erro ao salvar as alterações do Pull Request. Tente novamente ou contate o administrador.");
      return false;
    }

    return true;
  }

  private static string NormalizeText(string? text)
      => string.IsNullOrEmpty(text) ? string.Empty
         : text.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

  private static string ComputeSha512Hex(string input)
  {
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = SHA512.HashData(bytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
  }

  public void Dispose()
  {
    _azurePullRequestRepository?.Dispose();
  }
}