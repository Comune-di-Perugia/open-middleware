using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Query
{
  public class SearchDocuments : IRequest<IEnumerable<Dto.DocumentInfo>>
  {
    public string Pattern { get; set; }
    public int skip { get; set; }
    public int take { get; set; } = 100;
    public bool Exact { get; set; } = true;
  }
}
