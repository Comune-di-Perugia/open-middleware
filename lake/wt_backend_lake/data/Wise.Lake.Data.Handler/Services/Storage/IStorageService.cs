namespace Wisetown.Lake.Data.Handler.Services;

public interface IStorageService
{
  Task Set(Model.DataModel data, string containerName = null);
  Task<Stream> Get(string id, string containerName = null);
  Task Delete(string id, string containerName = null);
}
