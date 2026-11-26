using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Command
{
  public class Set : IRequest<Dto.DocumentInfo>
  {
    public string Id { get; set; }
    public string Position { get; set; }
    public string ContentType { get; set; }
    public string Name { get; set; }
    public long Size { get; set; }
  }
}
