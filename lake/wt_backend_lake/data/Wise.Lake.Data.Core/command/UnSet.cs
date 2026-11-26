using Hikyaku;

namespace Wisetown.Lake.Data.Core.Command;

public class UnSet : IRequest
{
  public string Id { get; set; }
  public string ContentType { get; set; }
}
