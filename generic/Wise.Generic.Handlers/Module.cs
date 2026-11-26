using System;
using System.Composition;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wise.Core;

[Export(typeof(IModule))]
public class Module : IModule
{
  public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostingEnvironment)
  {
  }

  public void OnStartup(IServiceProvider services)
  {
  }

  public void PostStartup(IServiceProvider services)
  {
  }

  public void UseEndpoints(IEndpointRouteBuilder endpoints)
  {
  }
}
