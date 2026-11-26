using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Wise.Core;

namespace Wise.Generic;

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
    Configure(app, builder.Environment, builder.Configuration);

    app.Run();
  }

  public static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IHostEnvironment environment)
  {
    services.AddCors(options => { options.AddDefaultPolicy(opt => opt.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(s => true).AllowCredentials()); });

    Loader.Current.ConfigureServices(services, configuration, environment);

    services.AddAuthorization(options => options.AddWiseDefaultPolicies());
    services.AddWiseAuthentication(opts => configuration.GetSection("JWT").Bind(opts));
    services.AddControllers()
      .AddNewtonsoftJson(options => options.SerializerSettings.ConfigureDefaults());

    services.AddOpenApi();

    services.AddHikyaku(cfg => { cfg.RegisterServicesFromAssemblies(Loader.Current.Assemblies.ToArray()); });
    services.AddKaido(opt =>
    {
      opt.Behaviour = Hikyaku.Kaido.HikyakuBehaviourEnum.ImplicitRemote;
      opt.InferLocalRequests(Loader.Current.Assemblies);
      opt.InferLocalNotifications(Loader.Current.Assemblies);
    }).AddKaidoPerformanceMonitor().AddHikyakuRabbitMQMessageDispatcher(o =>
    {
      configuration.GetSection("rabbitmq").Bind(o);
      o.ClientName = o.ClientName ?? Assembly.GetExecutingAssembly().GetName().Name;
    }).AddRabbitMQRequestManager();

    services.AddMapZilla(Loader.Current.Assemblies);
    services.AddVersionAndHealthChecks();
  }

  public static void Configure(WebApplication app, IHostEnvironment environment, IConfiguration configuration)
  {
    if (environment.IsDevelopment())
      app.UseDeveloperExceptionPage();

    app.UseRouting();
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    Action<ScalarOptions> configureOptions = options =>
      options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    app.MapScalarApiReference(configureOptions);

    Loader.Current.AddModules(app.Services);

    app.MapVersionController("version");
    app.MapControllers();
    foreach (var m in Loader.Current.Modules)
      m.UseEndpoints(app);

    app.MapOpenApi();

    app.UseDefaultFiles();
    app.UseStaticFiles();
  }
}
