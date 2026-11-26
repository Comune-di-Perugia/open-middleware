using System.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Wise.Core;

namespace Wisetown.Lake.Classification.Api
{
  [Export(typeof(IModule))]
  public class Module : IModule
  {
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
      // var mvcbuilder = services.AddControllers();
      // mvcbuilder.PartManager.ApplicationParts.Add(new AssemblyPart(Assembly.GetExecutingAssembly()));
      // mvcbuilder.AddNewtonsoftJson();
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