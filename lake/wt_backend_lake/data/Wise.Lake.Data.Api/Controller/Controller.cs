#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hikyaku;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using query = Wisetown.Lake.Data.Core.Query;
using command = Wisetown.Lake.Data.Core.Command;
using dto = Wisetown.Lake.Data.Core.Dto;
using System.Net;

namespace Wisetown.Lake.Data.Api.Controller;

[ApiController]
[Route("/api/data")]
public class DataController : ControllerBase
{
  private readonly IHikyaku _axon;
  public DataController(IHikyaku axon)
  {
    this._axon = axon;
  }

  [HttpGet("{id}")]
  [ProducesResponseType(404)]
  [ProducesResponseType(200)]
  public async Task<IActionResult> Get(string id)
  {
    var result = await this._axon.Send(new query.Get() { Id = id });
    if (result != null)
      return File(result.Data, result.ContentType);
    return NotFound();
  }

  [HttpGet("{id}/exists")]
  public async Task<bool> Exists(string id)
  {
    return await this._axon.Send(new query.Exists() { Id = id });
  }

  [HttpGet("{id}/info")]
  [ProducesResponseType(404)]
  [ProducesResponseType(typeof(dto.DocumentInfo), 200)]
  public async Task<IActionResult> GetInfo(string id)
  {
    var result = await this._axon.Send(new query.GetInfo() { Id = id });
    if (result != null) return Ok(result);
    return NotFound();
  }

  [HttpPost("{id}"), HttpPut("{id}")]
  [ProducesResponseType(400)]
  [Produces(typeof(IEnumerable<dto.DocumentInfo>))]
  public async Task<IActionResult> Set(string id, IFormFile file)
  {
    var result = new List<dto.DocumentInfo>();
    {
      if (file.Length == 0) return BadRequest();
      using var stream = file.OpenReadStream();
      var buffer = new Byte[file.Length];

      await stream.ReadExactlyAsync(buffer, 0, (int)file.Length);
      result.Add(await _axon.Send(new command.Set()
      {
        Id = id,
        ContentType = file.ContentType,
        Name = file.FileName,
        Data = buffer
      }));

    }
    return Ok(result);
  }

  [HttpPost]
  [ProducesResponseType(400)]
  [Produces(typeof(IEnumerable<dto.DocumentInfo>))]
  public async Task<IActionResult> Set(List<IFormFile> files)
  {
    var result = new List<dto.DocumentInfo>();
    foreach (var f in files)
    {
      if (f.Length == 0) return BadRequest();
      using var stream = f.OpenReadStream();
      var buffer = new Byte[f.Length];

      await stream.ReadExactlyAsync(buffer, 0, (int)f.Length);
      result.Add(await _axon.Send(new command.Set()
      {
        Id = Guid.NewGuid().ToString().Replace("-", ""),
        ContentType = f.ContentType,
        Name = f.FileName,
        Data = buffer
      }));

    }
    return Ok(result);
  }

  [HttpDelete("{id}")]
  [ProducesResponseType((int)HttpStatusCode.Accepted)]
  public async Task<IActionResult> Delete(string id)
  {
    await _axon.Send(new command.Delete { Id = id });
    // we return as accepted because of soft deletion
    return Accepted();
  }

}
