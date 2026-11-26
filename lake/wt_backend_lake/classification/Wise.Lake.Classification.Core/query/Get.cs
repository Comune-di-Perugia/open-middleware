using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Query
{
  public class Get : IRequest<Dto.DocumentInfo>
  {
    public string Id { get; set; }
    public bool IncludeTags { get; set; } = false;
  }
}
