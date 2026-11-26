
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Generic.api.Services.dto;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Generic.api.Services;

public class ClassificationClient : IClassificationClient
{
  private readonly MiddlewareSettings configuration;

  public ClassificationClient(IOptions<MiddlewareSettings> configuration)
  {
    this.configuration = configuration.Value;
  }

  public async Task AddTags(string id, params string[] tags)
  {
    using (var client = new HttpClient())
    {
      await client.PostAsync($"{configuration.IndexingServiceUri}/api/info/{id}/tags",
        new StringContent(JsonConvert.SerializeObject(tags), Encoding.UTF8, "application/json"));
    }
  }

  public async Task RemoveTags(string id, params string[] tags)
  {
    using (var client = new HttpClient())
    {
      var request = new HttpRequestMessage(HttpMethod.Delete, $"{configuration.IndexingServiceUri}/api/info/{id}/tags");
      request.Content = new StringContent(JsonConvert.SerializeObject(tags), Encoding.UTF8, "application/json");
      await client.SendAsync(request);
    }
  }

  public async Task<DocumentInfo> Info(string id)
  {
    using (var client = new HttpClient())
    {
      var result = await client.GetStringAsync($"{configuration.IndexingServiceUri}/api/info/{id}");
      return JsonConvert.DeserializeObject<DocumentInfo>(result);
    }
  }

  public async Task<string[]> SearchTags(string pattern, int? skip = null, int? take = null)
  {
    using (var client = new HttpClient())
    {
      var query = $"{configuration.IndexingServiceUri}/api/search/tags?pattern={pattern}";
      if (skip != null) query += $"&skip={skip}";
      if (take != null) query += $"&take={take}";
      var result = await client.GetStringAsync(query);
      return JsonConvert.DeserializeObject<string[]>(result);
    }
  }

  public async Task<DocumentInfo[]> SearchDocuments(string pattern, int skip = 0, int take = 100)
  {
    using (var client = new HttpClient())
    {
      var result = await client.GetStringAsync($"{configuration.IndexingServiceUri}/api/search/documents?pattern={pattern}&skip={skip}&take={take}");
      return JsonConvert.DeserializeObject<DocumentInfo[]>(result);
    }
  }

  public async Task<byte[]> GetDocument(string id)
  {

    using (var client = new HttpClient())
    {
      var result = await client.GetByteArrayAsync($"{configuration.IndexingServiceUri}/api/data/{id}");
      return result;
    }
  }
}
