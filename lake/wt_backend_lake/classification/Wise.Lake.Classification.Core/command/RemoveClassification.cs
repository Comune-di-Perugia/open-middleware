using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Command
{
  public class RemoveClassification : IRequest
  {
    public string Id { get; set; }

    public string[] Tags { get; set; }

  }
}
