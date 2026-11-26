namespace Wisetown.Lake.Data.Core.Dto;

public class Document
{
  public string Id { get; set; }

  public string ContentType { get; set; }

  public Stream Data { get; set; }
}
