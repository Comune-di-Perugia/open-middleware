using System.Composition;
using Hikyaku;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wise.Core;
using Wisetown.Lake.Classification.Handler.Model;

namespace Wisetown.Lake.Classification.Handler
{
  [Export(typeof(IModule))]
  public class Module : IModule
  {
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
      switch (configuration["DataBaseType"])
      {
        default:
        case "sqldb":
          services.AddDbContext<Model.SQLMigrationContext>((provider, options) =>
            options.UseSqlServer(configuration.GetConnectionString("sqldb"))
              .AddInterceptors(provider.GetRequiredService<SlowQueryInterceptor>()));
          services.AddDbContext<Model.DB>(options =>
            options.UseSqlServer(configuration.GetConnectionString("sqldb")));
          break;
      }


      services.AddScoped<IShardedService<Guid, DB>>(o => new ShardedService<Guid, string, DB>(o,
        async (provider, partitionkey) =>
        {
          var m = provider.GetRequiredService<IHikyaku>();
          return await m.Send(new Wise.Identity.Core.Query.GetShardName() { WorkspaceId = partitionkey });
        },
        (provider, shardName, db) =>
        {
          if (string.IsNullOrEmpty(shardName)) return Task.CompletedTask;
          var appSettings = provider.GetService<IConfiguration>();
          var connectionString = appSettings.GetConnectionString($"{appSettings["DataBaseType"]}-{shardName}");

          if (connectionString != null && (appSettings.GetValue<bool?>("ShardsEnabled") ?? false))
            db.Database.SetConnectionString(connectionString);

          return Task.CompletedTask;
        }
      ));


      // services.AddScoped<SlowQueryInterceptor>();
      // services.AddHostedService<NotificationsHandler>();
    }

    public async void OnStartup(IServiceProvider services)
    {
      using var serviceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope();
      try
      {
        DbContext context = null;
        var appSettings = serviceScope.ServiceProvider.GetService<IConfiguration>();
        switch (appSettings["DataBaseType"])
        {
          default:
          case "sqldb": context = serviceScope.ServiceProvider.GetService<Model.SQLMigrationContext>(); break;
        }

        if (context != null)
        {
          await context.Database.MigrateAsync();

          var ax = serviceScope.ServiceProvider.GetRequiredService<IHikyaku>();
          var shards = await ax.Send(new Wise.Identity.Core.Query.GetAvailableShards());
          if (shards != null)
            foreach (var s in shards)
            {
              if (string.IsNullOrEmpty(s)) continue;

              // Ensure connection is closed
              await context.Database.CloseConnectionAsync();
              var connectionString = appSettings.GetConnectionString($"{appSettings["DataBaseType"]}-{s}");

              if (connectionString != null && (appSettings.GetValue<bool?>("ShardsEnabled") ?? false))
              {
                context.Database.SetConnectionString(connectionString);
                await context.Database.MigrateAsync();
              }
            }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        throw;
      }
    }

    public void UseEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints)
    {
    }

    public void PostStartup(IServiceProvider services)
    {
    }
  }
}
