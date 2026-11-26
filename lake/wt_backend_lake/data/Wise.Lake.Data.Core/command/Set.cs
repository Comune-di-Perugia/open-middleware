using Hikyaku;

namespace Wisetown.Lake.Data.Core.Command
{
  public class Set : IRequest<Dto.DocumentInfo>
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
  }
}
