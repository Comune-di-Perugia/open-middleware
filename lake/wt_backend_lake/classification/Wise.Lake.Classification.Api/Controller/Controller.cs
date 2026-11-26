using Hikyaku;
using Microsoft.AspNetCore.Mvc;

using _query = Wisetown.Lake.Classification.Core.Query;
using _command = Wisetown.Lake.Classification.Core.Command;
using _dto = Wisetown.Lake.Classification.Core.Dto;

namespace Wisetown.Lake.Classification.Api;

[ApiController]
public class ClassificationsController : ControllerBase
{
  private readonly IHikyaku _axon;
  public ClassificationsController(IHikyaku axon)
  {
    this._axon = axon;
  }

  [HttpPost("~/api/info/{id}/tags")]
  [ProducesResponseType(200)]
  public async Task<IActionResult> Add(string id, [FromBody] string[] tags)
  {
    await this._axon.Send(new _command.AddClassification() { Id = id, Tags = tags });
    return Ok();
  }

  [HttpDelete("~/api/info/{id}/tags")]
  public async Task<IActionResult> Remove(string id, [FromBody] string[] tags)
  {
    await this._axon.Send(new _command.RemoveClassification() { Id = id, Tags = tags });
    return Ok();
  }

  [HttpGet("~/api/info/{id}")]
  [ProducesResponseType(404)]
  [ProducesResponseType(typeof(_dto.DocumentInfo), 200)]
  public async Task<IActionResult> Get(string id)
  {
    var result = await this._axon.Send(new _query.Get() { Id = id, IncludeTags = true });
    if (result == null) return NotFound();
    return Ok(result);
  }

  [HttpGet("~/api/search/tags")]
  [ProducesResponseType(typeof(string[]), 200)]
  public async Task<IActionResult> SearchTags(string pattern, int? skip = null, int? take = null)
  {
    var result = await this._axon.Send(new _query.SearchTags() { Pattern = pattern, skip = skip, take = take });
    return Ok(result);
  }

  [HttpGet("~/api/search/documents")]
  [ProducesResponseType(typeof(_dto.DocumentInfo[]), 200)]
  public async Task<IActionResult> SearchDocuments(string pattern, int skip = 0, int take = 100)
  {
    if (take > 1000) return BadRequest("take parameter cannot be bigger than 1000");
    var result = await this._axon.Send(new _query.SearchDocuments() { Pattern = pattern, skip = skip, take = take });
    return Ok(result);
  }

  [HttpGet("~/api/workspace/{workspaceid}/usage")]
  [ProducesResponseType(typeof(_dto.WorkspaceUsage), 200)]
  public async Task<IActionResult> GetWorkspaceUsage(Guid workspaceid)
  {
    var result = await this._axon.Send(new _query.GetWorkspaceUsage() { WorkspaceId = workspaceid });
    return Ok(result);
  }
}
