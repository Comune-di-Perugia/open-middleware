using Hikyaku;

namespace Wisetown.Lake.Data.Core.Query
{
  public class Get : IRequest<Dto.Document>
  {
    public string Id { get; set; }
  }
}
