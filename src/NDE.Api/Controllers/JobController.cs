using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

using NDE.Application.Workers;
using NDE.Domain.Entities.Common;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
public class JobController : MainController
{
  private readonly IJobTaskQueue _queue;
  private readonly JobWorkerStore _store;
  private readonly IDistributedCache _cache;
  private readonly ILogger<JobController> _logger;

  public JobController(
    IJobTaskQueue queue,
    JobWorkerStore store,
    IDistributedCache cache,
    ILogger<JobController> logger,
    INotificator notificator) : base(notificator)
  {
    _queue = queue;
    _store = store;
    _logger = logger;
    _cache = cache;
  }

  [HttpGet("status/{id:guid}")]
  public async Task<IActionResult> Status(Guid id)
  {
    var job = await _store.Get(id);
    if (job == null) return NotFound();

    return NDEResponse(new { status = job.Status });
  }

  [HttpGet("result/{id:guid}")]
  public async Task<IActionResult> Result(Guid id)
  {
    var job = await _store.Get(id);
    if (job == null) return NotFound();

    if (job.Status != JobStatus.Finished.Description) return BadRequest();

    return NDEResponse(new { result = job.Result });
  }

  [HttpPost("cancel/{id:guid}")]
  public async Task<IActionResult> Cancel(Guid id)
  {
    await _cache.SetStringAsync($"jobs:{id}:cancel", "1");
    return Ok();
  }
}
