using System;
using System.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Wise.Core;

namespace Wisetown.Lake.Data.Api
{
  [Export(typeof(IModule))]
  public class Module : IModule
  {
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
    }

    public void OnStartup(IServiceProvider services)
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