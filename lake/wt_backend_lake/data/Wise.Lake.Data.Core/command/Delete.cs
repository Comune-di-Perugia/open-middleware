using Hikyaku;

namespace Wisetown.Lake.Data.Core.Command
{
  public class Delete : IRequest
  {
    public string Id { get; set; }
  }
}
