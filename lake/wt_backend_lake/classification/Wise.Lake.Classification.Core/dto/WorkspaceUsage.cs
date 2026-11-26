namespace Wisetown.Lake.Classification.Core.Dto;

public class WorkspaceUsage
{
  public Guid WorkspaceId { get; set; }
  public int DocumentCount { get; set; }
  public long TotalSizeBytes { get; set; }
}

