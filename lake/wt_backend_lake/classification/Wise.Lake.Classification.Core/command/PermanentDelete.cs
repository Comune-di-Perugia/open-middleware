using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Command;

public class PermanentDelete : IRequest
{
  public string Id { get; set; }
}
