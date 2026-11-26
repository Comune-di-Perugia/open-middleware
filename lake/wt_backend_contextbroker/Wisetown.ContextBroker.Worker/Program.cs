using System.Reflection;
using FIWARE.Orion.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wise.Core;

namespace Wisetown.ContextBroker.Worker;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = Host.CreateDefaultBuilder(args);

    Loader.Current.Directories.Add(Directory.GetCurrentDirectory());
    Loader.Current.Compose();

    builder.ConfigureServices((buildercontext, services) =>
    {
      ConfigureServices(services,
      buildercontext.Configuration,
      buildercontext.HostingEnvironment);
    });

    var app = builder.Build();
    Configure(app);

    app.Run();
  }

  public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
  {
    Loader.Current.ConfigureServices(services, configuration, environment);

    services.AddHikyaku(cfg => { cfg.RegisterServicesFromAssemblies(Loader.Current.Assemblies.ToArray()); });
    services.AddMapZilla(Loader.Current.Assemblies);

    services.AddKaido(opt =>
      {
        opt.Behaviour = Hikyaku.Kaido.HikyakuBehaviourEnum.ImplicitRemote;

        opt.ListenForNotification<Wise.Tasks.Core.Notifications.TaskChanged>();
        opt.ListenForNotification<Wise.Tasks.Core.Notifications.TaskArchived>();
        opt.ListenForNotification<Wise.Tasks.Core.Notifications.TaskDeleted>();
        opt.ListenForNotification<Wise.Tasks.Core.Notifications.TaskCreated>();
        // opt.ListenForNotification<Wise.Tasks.Core.Notifications.TaskAccepted>();
        // opt.ListenForNotification<Wise.Tasks.Core.Notifications.TaskRefused>();
      }).AddKaidoPerformanceMonitor().AddHikyakuRabbitMQMessageDispatcher(o =>
      {
        configuration.GetSection("rabbitmq").Bind(o);
        o.ClientName = o.ClientName ?? Assembly.GetExecutingAssembly().GetName().Name;
      })
      .AddRabbitMQRequestManager();

    // Configure Orion Client 
    services.Configure<FIWARE.Orion.Client.OrionClient.OrionConfig>(i => configuration.GetSection("orion").Bind(i));
    services.AddScoped<FIWARE.Orion.Client.OrionClient>(i =>
    {
      var cfg = i.GetRequiredService<IOptions<FIWARE.Orion.Client.OrionClient.OrionConfig>>();
      return new OrionClient(cfg.Value);
    });
  }

  public static void Configure(IHost app)
  {
    Loader.Current.AddModules(app.Services);
  }
}
