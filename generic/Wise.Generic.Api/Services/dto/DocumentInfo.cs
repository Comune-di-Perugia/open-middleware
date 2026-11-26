using System;

namespace Generic.api.Services.dto;

public class DocumentInfo
{
  public string Id { get; set; }
  public string ContentType { get; set; }
  public string Position { get; set; }
  public DateTime IngestionDate { get; set; }
  public long Size { get; set; }
  public DateTime UpdateDate { get; set; }
  public bool Stoned { get; set; }
  public DateTime ExpirationDate { get; set; }
  public string StorageId { get; set; }
  public string[] Tags { get; set; }
  public string Name { get; set; }
}