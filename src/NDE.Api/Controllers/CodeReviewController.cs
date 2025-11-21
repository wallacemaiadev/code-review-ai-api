using System.Text.Json;

using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.Workers;
using NDE.Domain.Entities.Common;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
public class CodeReviewController : MainController
{
  private readonly ICodeReviewService _codeReviewService;
  private readonly IJobTaskQueue _queue;
  private readonly JobWorkerStore _store;
  private readonly IDistributedCache _cache;
  private readonly ILogger<CodeReviewController> _logger;

  public CodeReviewController(
    ICodeReviewService codeReviewService,
    IJobTaskQueue queue,
    JobWorkerStore store,
    IDistributedCache cache,
    ILogger<CodeReviewController> logger,
    INotificator notificator) : base(notificator)
  {
    _codeReviewService = codeReviewService;
    _queue = queue;
    _store = store;
    _logger = logger;
    _cache = cache;
  }


  [MapToApiVersion(1)]
  [HttpPost("start/review")]
  public async Task<IActionResult> StartReview([FromBody] StartReviewRequestViewModel request)
  {
    if (!ModelState.IsValid)
      return NDEResponse(ModelState);

    try
    {
      var job = await _store.Create();

      _queue.Queue(async scope =>
      {
        await _store.SetRunning(job.Id);

        var maxRetries = 3;
        var delay = TimeSpan.FromSeconds(3);

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
          if (await _store.IsCanceled(job.Id))
          {
            await _store.SetCanceled(job.Id);
            return;
          }

          try
          {
            var reviewService = scope.GetRequiredService<ICodeReviewService>();
            var result = await reviewService.ReviewAsync(request);
            await _store.SetResult(job.Id, JsonSerializer.Serialize(result));
            return;
          }
          catch (Exception ex)
          {
            var reachedMax = await _store.IncrementError(job.Id, maxRetries);

            _logger.LogError(
              ex,
              "Erro ao processar job {JobId}, tentativa {Attempt} de {MaxRetries}",
              job.Id,
              attempt + 1,
              maxRetries
            );

            if (reachedMax)
            {
              await _store.SetDead(job.Id);

              var dead = new JobStoreDeadLetter(job.Id, request, attempt + 1);

              await _cache.SetStringAsync(
                $"jobs:dead:{job.Id}",
                JsonSerializer.Serialize(dead));

              return;
            }

            await DelayWithCancellationCheck(job.Id, delay);
            delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
          }
        }

        await _store.SetDead(job.Id);

        var finalDead = new JobStoreDeadLetter(job.Id, request, maxRetries);

        await _cache.SetStringAsync(
          $"jobs:dead:{job.Id}",
          JsonSerializer.Serialize(finalDead));
      });

      return NDEResponse(new { jobId = job.Id, status = job.Status });
    }
    catch (Exception ex)
    {
      _logger.LogCritical(ex, "Houve um erro ao enfileirar job de code review");
      throw;
    }
  }

  [MapToApiVersion(1)]
  [HttpPut("update/review")]
  public async Task<IActionResult> UpdateReview([FromBody] UpdateCodeReviewRequestViewModel request)
  {
    if (!ModelState.IsValid)
      return NDEResponse(ModelState);

    return NDEResponse(await _codeReviewService.UpdateCodeReviewAsync(request));
  }

  [MapToApiVersion(1)]
  [HttpGet("get/review")]
  public async Task<IActionResult> GetReview([FromQuery] GetCodeReviewParamsQuery request)
  {
    if (!ModelState.IsValid)
      return NDEResponse(ModelState);

    var result = await _codeReviewService.GetAllCodeReviewAsync(
      repositoryId: request.ReposId,
      projectId: request.ProjectId,
      pullRequestId: request.PrId);

    return NDEResponse(result);
  }

  private async Task DelayWithCancellationCheck(Guid jobId, TimeSpan delay)
  {
    var step = TimeSpan.FromSeconds(1);
    var elapsed = TimeSpan.Zero;

    while (elapsed < delay)
    {
      if (await _store.IsCanceled(jobId))
        return;

      await Task.Delay(step);
      elapsed += step;
    }
  }
}
