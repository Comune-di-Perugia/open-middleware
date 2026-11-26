// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamDev.Redis;
using Wise.Core;
using Wisetown.Lake.Classification.Handler.Model;
using dto = Wisetown.Lake.Classification.Core.Dto;


var builder = Host.CreateDefaultBuilder(args);

Loader.Current.Directories.Add(Directory.GetCurrentDirectory());
Loader.Current.Compose();

builder.ConfigureServices((buildercontext, services) => { ConfigureServices(services, buildercontext.Configuration, buildercontext.HostingEnvironment); });

var app = builder.Build();
Configure(app);

var redis = GetRedisClient(app.Services.GetRequiredService<IConfiguration>());
var db = app.Services.GetRequiredService<DB>();


var commandid = redis.SendCommand(RedisCommand.KEYS, $"document::*");

var documents = redis.ReadMultiString();


foreach (var document in documents)
{
  if (!document.EndsWith("::tags"))
  {
    var rdocument = redis.Hash[document].Items.FromRedisDictionary<dto.DocumentInfo>();
    Console.WriteLine($"Evaluating {rdocument.Id} ");

    var ddocument = db.Documents.FirstOrDefault(i => i.Id == rdocument.Id);
    if (ddocument == null)
    {
      db.Documents.Add(new Document()
      {
        Id = rdocument.Id,
        Name = rdocument.Name,
        Position = rdocument.Position,
        Size = rdocument.Size,
        ContentType = rdocument.ContentType,
        IngestionDate = rdocument.IngestionDate,
        UpdateDate = rdocument.UpdateDate,
      });
      
      Console.WriteLine($"Document {rdocument.Id} has been created");
    }

    if (redis.Key.Exists($"{document}::tags"))
    {
      var tags = redis.Set[$"{document}::tags"].Members;
      var etags = db.DocumentTags.Where(i => i.Id == rdocument.Id).ToList();

      foreach (var t in tags)
      {
        if (etags.Find(i => i.Tag == t) == null)
        {
          db.DocumentTags.Add(new DocumentTag() { Id = rdocument.Id, Tag = t });
        }
      }
    }
    db.SaveChanges();
    db.ChangeTracker.Clear();
  }
}


// db.ChangeTracker.Clear();
// } while (cursor != 0);

Console.WriteLine("fine");

RedisClient GetRedisClient(IConfiguration configuration)
{
  var redis = new RedisClient();
  configuration.GetSection("redis").Bind(redis.Configuration);
  redis.Connect();
  return redis;
}

void ConfigureServices(IServiceCollection services, IConfiguration configuration,
  IHostEnvironment environment)
{
  Loader.Current.ConfigureServices(services, configuration, environment);
}

void Configure(IHost app)
{
  Loader.Current.AddModules(app.Services);
}