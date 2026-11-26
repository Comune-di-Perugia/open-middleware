using System;

namespace Wise.Generic.Core.Dtos;

public class SharedFile
{
  public string Token { get; set; }
}

public class SharedData
{
  public Guid WorkspaceId { get; set; }
  public string Id { get; set; }
  public string Context { get; set; }
  public DateTime? ExpirationDate { get; set; }
}
