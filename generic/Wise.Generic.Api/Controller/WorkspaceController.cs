using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hikyaku;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wise.Core;
using Wise.Web;
using Wisetown.Lake.Classification.Core.Query;

namespace Wise.Generic.Api.Controller;

[ApiController]
[Route("~/generic/workspace/")]
[ControllerGroupBy("generic")]
[ControllerGroupBy("generic-files")]
public class WorkspaceController(IHikyaku hikyaku): ControllerBase

{
  /// <summary>
  /// Retrieves the usage information for a workspace.
  /// </summary>
  /// <param name="id">The identifier of the workspace.</param>
  /// <returns>An IActionResult representing the result of the operation.</returns>
  /// <remarks>
  /// This method is an HTTP GET endpoint that requires the user to be authenticated as an Infrastructure Admin.
  /// It acts as a proxy to the Wisetown.Lake.Classification microservice, requesting usage information for the specified workspace.
  /// The result of the query is returned with an HTTP 200 (OK) status code along with the workspace usage details.
  /// </remarks>
  [HttpGet]
  [Route("{id:guid}/usage")]
  [Authorize(WisePolicies.ISInfrastructureAdmin)]
  public async Task<IActionResult> GetWorkspaceUsage(Guid id)
  {
    return Ok(await hikyaku.Send(new GetWorkspaceUsage()
    {
      WorkspaceId = id
    }));
  }

  [HttpGet] [Route("usage")] 
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> GetMyWorkspaceUsage()
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace) ?? Guid.Empty.ToString());
    return Ok(await hikyaku.Send(new GetWorkspaceUsage()
    {
      WorkspaceId = wspId
    }));
  }
}