using System.Threading.Tasks;
using Generic.api.Services.dto;

namespace Generic.api.Services;

public interface IClassificationClient
{
  Task AddTags(string id, params string[] tags);

  Task RemoveTags(string id, params string[] tags);

  Task<DocumentInfo> Info(string id);

  Task<string[]> SearchTags(string pattern, int? skip = null, int? take = null);

  Task<DocumentInfo[]> SearchDocuments(string pattern, int skip = 0, int take = 100);

  Task<byte[]> GetDocument(string id);
}