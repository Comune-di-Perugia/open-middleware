using System.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wise.Core;

namespace Wisetown.Lake.Data.Handler
{
  [Export(typeof(IModule))]
  public class Module : IModule
  {
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
      var stype = configuration.GetValue<string>("storage-type") ?? "file";
      switch (stype)
      {
        case "file":
          services.Configure<Settings.FileSystemStorageSettings>(o => configuration.GetSection("file-storage").Bind(o));
          services.AddTransient<Services.IStorageService, Services.FileStorageService>();
          break;
      }

      services.AddHostedService<Worker>();
    }

    public void OnStartup(IServiceProvider app)
    {
    }

    public void UseEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints)
    {
    }

    public void PostStartup(IServiceProvider services)
    {
    }
  }
}