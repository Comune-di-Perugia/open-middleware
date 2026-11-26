using Microsoft.Extensions.Options;

namespace Wisetown.Lake.Data.Handler.Services
{
  public class FileStorageService : IStorageService
  {
    private Settings.FileSystemStorageSettings _settings;

    public FileStorageService(IOptions<Settings.FileSystemStorageSettings> settings)
    {
      this._settings = settings.Value;
    }

    public Task Delete(string id, string containerName = null)
    {
      if (File.Exists(this.FullPath(id, containerName)))
        File.Delete(this.FullPath(id, containerName));
      return Task.CompletedTask;
    }

    public Task<Stream> Get(string id, string containerName = null)
    {
      var path = this.FullPath(id, containerName);
      if (File.Exists(path))
        return Task.FromResult(new FileStream(path, FileMode.Open, FileAccess.Read) as Stream);
      return Task.FromResult(null as Stream);
    }

    public async Task Set(Model.DataModel data, string containerName = null)
    {
      var path = this.FullPath(data.Id, containerName);

      var container = Path.GetDirectoryName(path);
      if (!Directory.Exists(container))
        Directory.CreateDirectory(container);

      await File.WriteAllBytesAsync(path, data.Data);
    }

    public string FullPath(string id, string containerName = null)
    {
      return Path.Combine(this._settings.RootPath, (containerName ?? _settings.DefaultContainer).ToLower(), id);
    }
  }
}
