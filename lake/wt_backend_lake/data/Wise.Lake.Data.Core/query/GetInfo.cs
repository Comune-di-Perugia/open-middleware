using Hikyaku;

namespace Wisetown.Lake.Data.Core.Query
{
  public class GetInfo : IRequest<Dto.DocumentInfo>
  {
    public string Id { get; set; }
  }
}
