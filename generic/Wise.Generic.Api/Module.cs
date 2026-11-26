using System;
using System.Composition;
using Generic.api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Wise.Core;

namespace Generic.api
{
  [Export(typeof(IModule))]
  public class Module : IModule
  {
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
      services.Configure<MiddlewareSettings>(o => configuration.GetSection("middleware").Bind(o));
      services.AddTransient<IClassificationClient, ClassificationClient>();
    }

    public void OnStartup(IServiceProvider services)
    {
      
    }

    public void UseEndpoints(IEndpointRouteBuilder endpoints)
    {
      
    }

    public void PostStartup(IServiceProvider services)
    {
      
    }
  }
}