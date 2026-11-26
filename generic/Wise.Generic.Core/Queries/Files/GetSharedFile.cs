using System;
using Hikyaku;

namespace Wise.Generic.Core.Queries;

public class GetSharedFile : IRequest<Dtos.SharedFile>
{
  public Guid WorkspaceId { get; set; }
	public string Id { get; set; }
	public string Context { get; set; }
	public float? SLifetime { get; set; }
}