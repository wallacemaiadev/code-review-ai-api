using Microsoft.Extensions.Logging;

using NDE.Application.Interfaces;
using NDE.Application.Validators;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Application.Services;

public class PullRequestService : BaseService, IPullRequestService
{
  private readonly IPullRequestRepository _pullRequestRepository;
  private readonly IRepositoryRepository _repositoryRepository;
  private readonly IProjectRepository _projectRepository;
  private readonly ILogger<PullRequestService> _logger;

  public PullRequestService(
      IPullRequestRepository pullRequestRepository,
      IRepositoryRepository repositoryRepository,
      IProjectRepository projectRepository,
      ILogger<PullRequestService> logger,
      INotificator notificator) : base(notificator)
  {
    _pullRequestRepository = pullRequestRepository;
    _repositoryRepository = repositoryRepository;
    _projectRepository = projectRepository;
    _logger = logger;
  }


  public async Task<PullRequest?> CreateAsync(PullRequest pullRequest)
  {
    if (!Validate(new PullRequestValidator(), pullRequest))
    {
      _logger.LogError("Houve um erro ao tentar processar a solicitação de PR: {@pullRequest}", pullRequest);
      return null;
    }

    var repository = await _repositoryRepository.GetById(pullRequest.RepositoryId);
    if (repository == null)
    {
      Notify("O repositório com o ID informado não existe.");
      return null;
    }

    var pullRequestExists = await _pullRequestRepository.GetPullRequestAsync(repositoryId: pullRequest.RepositoryId, pullRequestId: pullRequest.PullRequestId);
    if (pullRequestExists != null)
    {
      return pullRequestExists;
    }

    await _pullRequestRepository.Add(pullRequest);

    if (!Commit(await _pullRequestRepository.SaveChangesAsync()))
    {
      Notify("Houve um erro ao tentar cadastrar o Pull Request.");
      return null;
    }

    AppMetrics.ObservePullRequestCreated(
      pullRequestId: pullRequest.PullRequestId,
      repositoryId: repository.Id,
      projectId: repository.ProjectId,
      repositoryName: repository.Name
    );

    return pullRequest;
  }

  public async Task<PullRequest?> GetAsync(Guid repositoryId, int? pullRequestId = null, Guid? id = null, bool includeCodeReviews = false)
  {
    if (repositoryId == Guid.Empty)
    {
      Notify("Você precisa informar o ID do repositório.");
      return null;
    }

    if (id == null && pullRequestId == null)
    {
      Notify("Você precisa informar o ID interno do Pull Request ou o ID do Pull Request da plataforma.");
      return null;
    }

    return await _pullRequestRepository.GetPullRequestAsync(id: id, repositoryId: repositoryId, pullRequestId: pullRequestId, includeChildren: includeCodeReviews);
  }

  public async Task<int> UpdateAsync(PullRequest pullRequest)
  {
    if (!Validate(new PullRequestValidator(), pullRequest))
    {
      _logger.LogError("Houve um erro ao tentar atualizar o PR: {@pullRequest}", pullRequest);
      return -1;
    }

    var repository = await _repositoryRepository.GetById(id: pullRequest.RepositoryId);
    if (repository == null)
    {
      Notify("O repositório com o Id informado não existe.");
      return -1;
    }

    var project = await _projectRepository.GetById(id: repository.ProjectId);
    if (project == null)
    {
      Notify($"Houve um erro ao tentar obter o projeto do repositório {repository.Id}");
      return -1;
    }

    _pullRequestRepository.Update(pullRequest);

    if (!Commit(await _pullRequestRepository.SaveChangesAsync()))
    {
      Notify("Houve um erro ao tentar cadastrar o Pull Request.");
      return -1;
    }

    AppMetrics.ObservePullRequestUpdated(
      pullRequestId: pullRequest.PullRequestId,
      title: pullRequest.Title,
      projectName: project.Name,
      repositoryName: repository.Name,
      projectId: project.Id,
      repositoryId: repository.Id
      );

    return pullRequest.PullRequestId;
  }
}