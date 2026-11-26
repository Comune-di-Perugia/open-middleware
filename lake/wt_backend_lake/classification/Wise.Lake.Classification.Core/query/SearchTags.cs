using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Query
{
  public class SearchTags : IRequest<IEnumerable<string>>
  {
    public string Pattern { get; set; }

    public int? skip { get; set; }
    public int? take { get; set; }
  }
}
