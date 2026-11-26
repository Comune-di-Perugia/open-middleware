using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Generic.api.Services;
using Hikyaku;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Wise.Core;
using Wise.Web;
using Wisetown.Lake.Core.Commands;

namespace Generic.api.Controller;

[ApiController]
[Route("~/g/{context}/")]
[Route("~/generic/{context}/")]
[ControllerGroupBy("generic")]
[ControllerGroupBy("generic-data")]
public class DataController : ControllerBase
{
  private readonly IHikyaku axon;
  private readonly IClassificationClient classificationClient;

  public DataController(
    IHikyaku axon,
    IClassificationClient classificationClient)
  {
    this.axon = axon;
    this.classificationClient = classificationClient;
  }

  [HttpPost("data"), HttpPut("data/{id}")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> Set(string context, [FromBody] dynamic model, string id = null, string name = null)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

    var msg = new StoreMessage
    {
      Context = context,
      BodyType = DocumentBodyEnum.Embedded,
      DocumentName = name,
      ContentType = "application/json",
      ID = id ?? Guid.NewGuid().ToString().Replace("-", ""),
      RootClassification = $"{wspId}-context:{context}",
      DocumentBody = JsonConvert.SerializeObject(model),
      WorkspaceId = wspId,
      UserId = userId
    };

    await axon.Send(msg);
    return Ok(new { id = msg.ID });
  }

  [HttpPost("w/{workspaceid}/data")]
  [HttpPost("workspace/{workspaceid}/data")]
  [HttpPut("w/{workspaceid}/data/{id}")]
  [HttpPut("workspace/{workspaceid}/data/{id}")]
  public async Task<IActionResult> SetPublic(string context, Guid workspaceid, [FromBody] dynamic model,
    string id = null, string name = null)
  {
    var msg = new StoreMessage
    {
      Context = context,
      BodyType = DocumentBodyEnum.Embedded,
      DocumentName = name,
      ContentType = "application/json",
      ID = id ?? Guid.NewGuid().ToString().Replace("-", ""),
      RootClassification = $"{workspaceid}-context:{context}",
      DocumentBody = JsonConvert.SerializeObject(model),
      WorkspaceId = workspaceid,
    };

    await axon.Send(msg);
    return Ok(new { id = msg.ID });
  }

  [HttpDelete("{id}")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> Delete(string context, string id)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var info = await classificationClient.Info(id);
    if (info == null) return NotFound();

    if (!info.Tags.Contains($"{wspId}-context:{context}"))
      return NotFound();

    // Deleting item RootClassification
    await classificationClient.RemoveTags(id, $"{wspId}-context:{context}");

    // If no other context ar binded to the object, try to Stone it
    if (!info.Tags.Any(t => t != $"{wspId}-context:{context}" && t.Contains("-context:")))
    {
      await axon.Send(new StoneMessage
      {
        ID = id
      });
    }

    return Ok();
  }

  [HttpGet("data/{id}")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> Get(string context, string id = null)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));

    var info = await classificationClient.Info(id);
    if (info == null) return NotFound();

    if (!info.Tags.Contains($"{wspId}-context:{context}"))
      return NotFound();

    var result = await classificationClient.GetDocument(id);
    if (result == null) return NotFound();

    return this.File(result, info.ContentType);
  }

  [HttpPatch("data/{id}")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> AddContext(string context, string id, string newcontext)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    await axon.Send(new Wisetown.Lake.Classification.Core.Command.AddClassification()
    {
      Id = id,
      Tags = [$"{wspId}-context:{newcontext}"]
    });

    return Ok();
  }

  [HttpGet("data")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> List(string context, int skip = 0, int take = 100)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var result = (await classificationClient.SearchDocuments($"{wspId}-context:{context}", skip, take))
      .Where(d => d.Tags.Contains($"{wspId}-context:{context}"))
      .Select(d => new { d.Id, d.Size, d.ContentType, d.Tags, d.Name });
    return Ok(result);
  }
}
