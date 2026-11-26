using Hikyaku;

namespace Wisetown.Lake.Classification.Core.Query
{
  public class GetWorkspaceUsage : IRequest<Dto.WorkspaceUsage>
  {
    public Guid WorkspaceId { get; set; }
  }
}

