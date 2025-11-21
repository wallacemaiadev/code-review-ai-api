using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;

using NDE.Domain.Entities.Common;

namespace NDE.Application.Workers
{
  public class JobWorkerStore
  {
    private readonly IDistributedCache _cache;

    public JobWorkerStore(IDistributedCache cache)
    {
      _cache = cache;
    }

    public async Task<JobStore> Create()
    {
      var job = new JobStore
      {
        Id = Guid.NewGuid(),
        Status = JobStatus.Pending.Description,
        Attempts = 0
      };

      await Save(job);

      return job;
    }

    public async Task<JobStore?> Get(Guid id)
    {
      var key = $"jobs:{id}";
      var data = await _cache.GetAsync(key);
      if (data == null || data.Length == 0) return null;

      try
      {
        return JsonSerializer.Deserialize<JobStore>(data);
      }
      catch
      {
        await _cache.RemoveAsync(key);
        return null;
      }
    }

    public async Task SetRunning(Guid id)
    {
      var job = await Get(id);
      if (job == null) return;
      job.Status = JobStatus.Running.Description;
      await Save(job);
    }

    public async Task UpdateProgress(Guid id, int value)
    {
      var key = $"jobs:{id}:progress";
      var options = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromDays(7))
        .SetSlidingExpiration(TimeSpan.FromDays(7));

      await _cache.SetStringAsync(key, value.ToString(), options);
    }

    public async Task SetError(Guid id)
    {
      var job = await Get(id);
      if (job == null) return;
      job.Attempts++;
      job.Status = JobStatus.Error.Description;
      await Save(job);
    }

    public async Task SetCanceled(Guid id)
    {
      var job = await Get(id);
      if (job == null) return;
      job.Status = JobStatus.Canceled.Description;
      await Save(job);
    }

    public async Task SetResult(Guid id, string result)
    {
      var job = await Get(id);
      if (job == null) return;
      job.Status = JobStatus.Finished.Description;
      job.Result = result;
      await Save(job);
    }

    public async Task<bool> IsCanceled(Guid id)
    {
      var exists = await _cache.GetAsync($"jobs:{id}:cancel");
      return exists != null;
    }

    public async Task<bool> IncrementError(Guid id, int maxAttempts)
    {
      var job = await Get(id);
      if (job == null) return false;

      job.Attempts++;

      if (job.Attempts >= maxAttempts)
        job.Status = JobStatus.Dead.Description;
      else
        job.Status = JobStatus.Error.Description;

      await Save(job);

      return job.Attempts >= maxAttempts;
    }

    public async Task SetDead(Guid id)
    {
      var job = await Get(id);
      if (job == null) return;
      job.Status = JobStatus.Dead.Description;
      await Save(job);
    }

    private async Task Save(JobStore job)
    {
      var key = $"jobs:{job.Id}";
      var data = JsonSerializer.SerializeToUtf8Bytes(job);

      var options = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromDays(7))
        .SetSlidingExpiration(TimeSpan.FromDays(7));

      await _cache.SetAsync(key, data, options);
    }
  }
}
