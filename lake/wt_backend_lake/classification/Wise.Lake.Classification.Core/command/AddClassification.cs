using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Command
{
  public class AddClassification : IRequest
  {
    public string Id { get; set; }

    public string[] Tags { get; set; }

  }
}
