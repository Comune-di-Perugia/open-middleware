using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wise.Core;

namespace Wisetown.Lake;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    Loader.Current.Directories.Add(Directory.GetCurrentDirectory());
    Loader.Current.Compose();

    ConfigureServices(builder.Services,
      builder.Configuration,
      builder.Environment);

    var app = builder.Build();
    Configure(app, builder.Environment);

    app.Run();
  }

  public static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IHostEnvironment environment)
  {
    services.AddCors(options => { options.AddDefaultPolicy(opt => opt.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(s => true).AllowCredentials()); });

    Loader.Current.ConfigureServices(services, configuration, environment);

    services.AddAuthorization(options => options.AddWiseDefaultPolicies());
    services.AddWiseAuthentication(opts => configuration.GetSection("JWT").Bind(opts));
    services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ConfigureDefaults());
    services.AddOpenApi();
    services.AddHikyaku(cfg => { cfg.RegisterServicesFromAssemblies(Loader.Current.Assemblies.ToArray()); });
    services.AddMapZilla(Loader.Current.Assemblies);

    services.AddHealthChecks();

    services.AddKaido(opt =>
      {
        opt.Behaviour = Hikyaku.Kaido.HikyakuBehaviourEnum.ImplicitRemote;
        opt.InferLocalRequests(Loader.Current.Assemblies.ToArray());
        opt.InferLocalNotifications(Loader.Current.Assemblies.ToArray());
      })
      .AddKaidoPerformanceMonitor().AddHikyakuRabbitMQMessageDispatcher(o =>
      {
        configuration.GetSection("rabbitmq").Bind(o);
        o.ClientName = o.ClientName ?? Assembly.GetExecutingAssembly().GetName().Name;
      })
      .AddRabbitMQRequestManager();
  }

  public static void Configure(WebApplication app, IHostEnvironment environment)
  {
    if (environment.IsDevelopment())
      app.UseDeveloperExceptionPage();

    app.UseRouting();
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCors();

    Loader.Current.AddModules(app.Services);

    app.MapVersionController("version");
    app.MapControllers();
    foreach (var m in Loader.Current.Modules)
      m.UseEndpoints(app);
  }
}
