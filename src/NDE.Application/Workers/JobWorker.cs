using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NDE.Application.Workers
{
  public interface IJobTaskQueue
  {
    void Queue(Func<IServiceProvider, Task> workItem);
    Task<Func<IServiceProvider, Task>> DequeueAsync(CancellationToken cancellationToken);
  }

  public class JobTaskQueue : IJobTaskQueue
  {
    private readonly Channel<Func<IServiceProvider, Task>> _queue =
      Channel.CreateUnbounded<Func<IServiceProvider, Task>>();

    public void Queue(Func<IServiceProvider, Task> workItem)
    {
      if (workItem is null)
        throw new ArgumentNullException(nameof(workItem));

      if (!_queue.Writer.TryWrite(workItem))
        throw new InvalidOperationException("Job queue is not accepting items.");
    }

    public async Task<Func<IServiceProvider, Task>> DequeueAsync(CancellationToken cancellationToken)
      => await _queue.Reader.ReadAsync(cancellationToken);
  }

  public class JobWorker : BackgroundService
  {
    private readonly IServiceProvider _provider;
    private readonly IJobTaskQueue _queue;
    private readonly ILogger<JobWorker> _logger;

    public JobWorker(IServiceProvider provider, IJobTaskQueue queue, ILogger<JobWorker> logger)
    {
      _provider = provider;
      _queue = queue;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          var workItem = await _queue.DequeueAsync(stoppingToken);

          using var scope = _provider.CreateScope();
          var scopedProvider = scope.ServiceProvider;

          await workItem(scopedProvider);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
          break;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Houve um erro ao tentar executar o worker");
        }
      }
    }
  }
}
