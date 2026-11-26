using Hikyaku;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wisetown.Lake.Data.Handler
{
  public class Worker : IHostedService, IDisposable
  {
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger logger;
    private Timer timer;

    public Worker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<Worker> logger)
    {
      this.serviceProvider = serviceProvider;
      this.configuration = configuration;
      this.logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      logger.LogInformation("TrashCollector Hosted Service is starting.");

      this.timer = new Timer(this.CleanTrash, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      logger.LogInformation("TrashCollector Hosted Service is stopping.");

      this.timer?.Change(Timeout.Infinite, 0);

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      this.timer?.Dispose();
    }

    private async void CleanTrash(object state)
    {
      using var scope = serviceProvider.CreateAsyncScope();
      var axon = scope.ServiceProvider.GetService<IHikyaku>();
      var maxTrashDays = configuration.GetSection("storage")?.GetValue<int?>("maxTrashDays") ?? 15;

      // Scan all trashed data
      var trashed = await axon.Send(new Classification.Core.Query.GetTrashed());
      logger.LogInformation($"Found {trashed.Count()} items in trash...");

      foreach (var item in trashed)
      {
        if ((DateTime.Now - item.StoneDate) > TimeSpan.FromDays(maxTrashDays))
        {
          logger.LogInformation($"Permanently Deleting document {item.Id}...");

          // removes item from physical storage...
          await axon.Send(new Core.Command.UnSet
          {
            ContentType = item.ContentType,
            Id = item.Id,
          });

          // removes permanently document informations
          await axon.Send(new Classification.Core.Command.PermanentDelete { Id = item.Id });
        }
      }
    }
  }
}
