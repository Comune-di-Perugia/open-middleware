using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Command
{
  public class Delete : IRequest
  {
    public string Id { get; set; }
  }
}
