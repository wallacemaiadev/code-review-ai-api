using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

using NDE.Application.Integrations.Models;
using NDE.Application.ViewModels.Response.CodeReviews;

namespace NDE.Application.Integrations.Azure;

public class AzureIntegration : IAzureIntegration
{
  private readonly ILogger<AzureIntegration> _logger;
  private readonly string _markSuggestion = "By: Automated Review";

  public AzureIntegration(ILogger<AzureIntegration> logger)
  {
    _logger = logger;
  }

  public async Task<int> AddCodeReviewToPR(
    string collectionUrl,
    string projectName,
    Guid repositoryId,
    string portalUrl,
    string repositoryUrl,
    string token,
    int pullRequestId,
    List<LLMOutputResponseViewModel> reviews)
  {
    _logger.LogInformation("Iniciando adição de reviews ao PR #{PullRequestId}", pullRequestId);

    if (reviews == null || reviews.Count == 0)
      return 0;

    await CloseExistingCommentsAsync(
        collectionUrl: collectionUrl,
        projectName: projectName,
        repositoryId: repositoryId,
        token: token,
        pullRequestId: pullRequestId);

    var sb = new StringBuilder();

    for (int i = 0; i < reviews.Count; i++)
    {
      var r = reviews[i];

      sb.Append(RenderReviewBlock(
          repositoryUrl: repositoryUrl,
          pullRequestId: pullRequestId,
          filePath: r.FilePath,
          fileLink: r.FileLink,
          suggestion: r.Suggestion,
          verdict: r.Verdict));

      if (i < reviews.Count - 1)
        sb.Append("\n\n---\n\n");
    }

    sb.Append("\n\n");
    sb.Append("# Portal: ");
    sb.Append(portalUrl);
    sb.Append("\n\n");

    sb.Append(_markSuggestion);

    var body = sb.ToString();

    if (string.IsNullOrWhiteSpace(body))
      return 0;

    var git = await GetGitClientAsync(collectionUrl, token);
    var pr = await git.GetPullRequestAsync(projectName, repositoryId, pullRequestId);

    if (pr == null)
      return 0;

    var repoId = pr.Repository?.Id.ToString();
    if (string.IsNullOrEmpty(repoId))
      return 0;

    var thread = new GitPullRequestCommentThread
    {
      Status = CommentThreadStatus.Active,
      Comments = new List<Comment>
        {
            new Comment
            {
                Content = body,
                CommentType = CommentType.Text
            }
        }
    };

    var created = await git.CreateThreadAsync(thread, repoId, pullRequestId);
    return created.Id;
  }


  public async Task CloseExistingCommentsAsync(string collectionUrl, string projectName, Guid repositoryId, string token, int pullRequestId)
  {
    try
    {
      _logger.LogInformation("Iniciando fechamento de comentários existentes no PR #{PullRequestId}", pullRequestId);

      var git = await GetGitClientAsync(collectionUrl, token);
      var pr = await git.GetPullRequestAsync(projectName, repositoryId, pullRequestId);

      if (pr == null)
      {
        var msg = "Houve um erro ao tentar buscar o PR";
        _logger.LogError(msg);
        throw new Exception(msg);
      }

      var repoId = pr.Repository?.Id;
      if (repoId == null)
      {
        var msg = "Houve um erro ao tentar obter o ID do repositório.";
        _logger.LogError(msg);
        throw new Exception(msg);
      }

      var threads = await git.GetThreadsAsync(repoId.ToString(), pullRequestId);

      var myThreads = threads
        .Where(t => t.Comments != null &&
                    t.Comments.Any(c => (c.Content ?? string.Empty).Contains(_markSuggestion)))
        .ToList();

      if (!myThreads.Any())
      {
        _logger.LogInformation("Nenhum comentário existente encontrado para fechar");
        return;
      }

      _logger.LogInformation("Encontrados {Count} comentários para fechar", myThreads.Count);

      var success = 0;
      var errors = 0;

      foreach (var thread in myThreads)
      {
        try
        {
          if (thread.Id == 0)
          {
            _logger.LogWarning("Thread sem ID encontrada, pulando...");
            continue;
          }

          var update = new GitPullRequestCommentThread
          {
            Status = CommentThreadStatus.Closed,
            IsDeleted = false
          };

          await git.UpdateThreadAsync(update, repoId.ToString(), pullRequestId, thread.Id);

          success++;
          _logger.LogInformation("Thread {Id} fechada com sucesso", thread.Id);
        }
        catch (Exception ex)
        {
          errors++;
          _logger.LogError(ex, "Erro ao fechar thread {Id}", thread.Id);
        }
      }

      _logger.LogInformation("Fechamento concluído: {Success} sucesso(s), {Errors} erro(s)", success, errors);

      if (errors > 0 && success == 0)
        throw new Exception($"Falha ao fechar todos os {myThreads.Count} comentários");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao fechar comentários existentes no PR #{PullRequestId}", pullRequestId);
      throw;
    }
  }

  public async Task<AzurePullRequestInfoResponse> GetPullRequestInfoAsync(string collectionUrl, string projectName, Guid repositoryId, string token, int pullRequestId)
  {
    try
    {
      _logger.LogInformation("Buscando informações do PR #{PullRequestId}", pullRequestId);

      var git = await GetGitClientAsync(collectionUrl, token);
      var pr = await git.GetPullRequestAsync(projectName, repositoryId, pullRequestId);

      if (pr == null || string.IsNullOrWhiteSpace(pr.Title))
      {
        var msg = "Não foi possível obter o título do Pull Request.";
        _logger.LogError(msg);
        throw new Exception(msg);
      }

      var description = pr.Description ?? string.Empty;

      _logger.LogInformation("Título do PR obtido: \"{Title}\"", pr.Title);
      _logger.LogInformation("Descrição do PR obtida com tamanho: {Length}", description.Length);

      return new AzurePullRequestInfoResponse
      {
        Title = pr.Title,
        Description = description,
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao obter informações do PR #{PullRequestId}", pullRequestId);
      throw;
    }
  }

  private string RenderReviewBlock(
    string repositoryUrl,
    int pullRequestId,
    string filePath,
    string fileLink,
    string suggestion,
    string verdict)
  {
    var safe = filePath
      .Replace("\\", "\\\\")
      .Replace("[", "\\[")
      .Replace("`", "\\`");

    var mdLink = $"[{safe}]({fileLink})";

    var title = $"## Arquivo: {mdLink}\n\n";
    var ai = $"\n{suggestion}\n\n";

    return $"{title}{ai}";
  }

  private async Task<GitHttpClient> GetGitClientAsync(string collectionUrl, string token)
  {
    var uri = new Uri(collectionUrl);
    var creds = new VssBasicCredential(string.Empty, token);
    var connection = new VssConnection(uri, creds);
    return await connection.GetClientAsync<GitHttpClient>();
  }
}
