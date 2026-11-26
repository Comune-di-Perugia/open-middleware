using System.ComponentModel.DataAnnotations;

namespace Wisetown.Lake.Classification.Handler.Model;

public class Document
{
  [Key, StringLength(256)] public string Id { get; set; }
  [StringLength(2000)] public string Position { get; set; }
  [StringLength(200)] public string ContentType { get; set; }
  [StringLength(200)] public string Name { get; set; }
  public long Size { get; set; }
  public DateTime IngestionDate { get; set; }
  public DateTime UpdateDate { get; set; }
  public bool Stoned { get; set; }
  public DateTime? StoneDate { get; set; }
}