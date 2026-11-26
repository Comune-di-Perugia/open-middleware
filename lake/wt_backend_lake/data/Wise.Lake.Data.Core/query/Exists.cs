using Hikyaku;

namespace Wisetown.Lake.Data.Core.Query
{
  public class Exists : IRequest<bool>
  {
    public string Id { get; set; }
  }
}
