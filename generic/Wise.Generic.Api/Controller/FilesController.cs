using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hikyaku;
using Generic.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using Wise.Core;
using Wise.Web;
using Wisetown.Lake.Core.Commands;
using Wise.Generic.Core.Queries;
using Wise.Generic.Core.Dtos;

namespace Generic.api.Controller;

[ApiController]
[Route("~/generic/{context}/")]
[ControllerGroupBy("generic")]
[ControllerGroupBy("generic-files")]
public class FilesController : ControllerBase
{
  private readonly IHikyaku axon;
  private readonly IHostEnvironment environment;
  private readonly MiddlewareSettings settings;
  private readonly IClassificationClient classificationClient;
  private readonly ILogger<FilesController> logger;
  private byte[] key = null;
  private byte[] iv = null;
  
  public FilesController(
    IHikyaku axon,
    IHostEnvironment environment,
    IClassificationClient classificationClient,
    IOptions<MiddlewareSettings> settings,
    IConfiguration config, ILogger<FilesController> logger)
  {
    this.axon = axon;
    this.environment = environment;
    this.settings = settings.Value;
    this.classificationClient = classificationClient;
    this.logger = logger;

    try
    {
      key = Base64UrlTextEncoder.Decode(config.GetValue<string>("encryption:key"));
      iv = Base64UrlTextEncoder.Decode(config.GetValue<string>("encryption:iv"));
    }
    catch (Exception ex)
    {
      logger.LogDebug("Error reading appsettings 'encryption:key' or 'encryption:iv'. using default values.");
      logger.LogError(ex.Message);
    }
  }

  [Authorize(WisePolicies.NeedsWorkspace)]
  [HttpPost("file/{id}"), HttpPut("file/{id}")]
  public async Task<IActionResult> Upload(string context, string id, IFormFile file)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
    using var stream = file.OpenReadStream();
    var buffer = new Byte[file.Length];
    await stream.ReadExactlyAsync(buffer, 0, (int)file.Length);
    await this.SetCachedFile(buffer, id);

    await axon.Send(new StoreMessage()
    {
      Context = context,
      BodyType = DocumentBodyEnum.ExternalUri,
      DocumentName = file.FileName,
      ContentType = file.ContentType,
      ID = id,
      RootClassification = $"{wspId}-context:{context}",
      NotifyToMiddleware = true,
      DocumentUri = $"{settings.IngestionServiceUri}/api/i/{id}/once",
      NotificationTopic = new string[] { context, $"ContentType:{file.ContentType}" }.Join("."),
      WorkspaceId = wspId,
      UserId = userid
    });
    return Ok(new { id = id });
  }

  private const long MBytes = 1024 * 1024;
  private const long GBytes = 1024 * MBytes;
  private const long MaxFileSize = 10 * 1024L * 1024L * 1024L;

  [HttpPost("files")]
  [RequestSizeLimit(MaxFileSize)]
  [DisableFormValueModelBindingAttribute]
  [Authorize(WisePolicies.NeedsWorkspace)]
  [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
  public async Task<IActionResult> Upload(string context, List<IFormFile> files)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
    var ids = new List<dynamic>();
    foreach (var file in files)
    {
      var id = Guid.NewGuid().ToString().Replace("-", "");
      using var stream = file.OpenReadStream();
      await this.SetCachedFile(stream, id);

      await axon.Send(new StoreMessage
      {
        Context = context,
        BodyType = DocumentBodyEnum.ExternalUri,
        DocumentName = file.FileName,
        ContentType = file.ContentType,
        ID = id,
        RootClassification = $"{wspId}-context:{context}",
        NotifyToMiddleware = true,
        DocumentUri = $"{settings.IngestionServiceUri}/api/i/{id}/once",
        WorkspaceId = wspId,
        UserId = userid
      });

      ids.Add(new { id = id });
    }

    return Ok(ids);
  }

  [HttpPost("file")]
  [RequestSizeLimit(MaxFileSize)]
  [DisableFormValueModelBindingAttribute]
  [Authorize(WisePolicies.NeedsWorkspace)]
  [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
  public async Task<IActionResult> UploadBigFile(string context, IFormFile file)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var userid = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
    var id = Guid.NewGuid().ToString().Replace("-", "");
    using var stream = file.OpenReadStream();
    await this.SetCachedFile(stream, id);

    logger.LogInformation($"{settings.IngestionServiceUri}/api/i/{id}/once");

    await axon.Send(new StoreMessage
    {
      Context = context,
      BodyType = DocumentBodyEnum.ExternalUri,
      DocumentName = file.FileName,
      ContentType = file.ContentType,
      ID = id,
      RootClassification = $"{wspId}-context:{context}",
      NotifyToMiddleware = true,
      DocumentUri = $"{settings.IngestionServiceUri}/api/i/{id}/once",
      WorkspaceId = wspId,
      UserId = userid,
    });

    return Ok(new { id = id });
  }

  [HttpGet("file/{id}")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> GetFile(string context, string id)
  {
    var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));
    var info = await classificationClient.Info(id);
    if (info == null)
      return NotFound();

    if (!info.Tags.Contains($"{wspId}-context:{context}")) return NotFound();
    var file = await classificationClient.GetDocument(id);
    if (file == null)
      return NotFound();

    return File(file, info.ContentType);
  }

  [HttpGet("~/generic/shared")]
  public async Task<IActionResult> GetSharedFile(string token)
  {
    var cyper = Base64UrlTextEncoder.Decode(token);
    var jsondata = Wise.Generic.Core.CryptoUtils.DecryptData(cyper, key, iv);
    var shared = JsonConvert.DeserializeObject<SharedData>(jsondata);
    if (shared is null) return BadRequest("Invalid share");

    if (shared.ExpirationDate != null && shared.ExpirationDate < DateTime.Now)
      return BadRequest("Expired share");

    var info = await classificationClient.Info(shared.Id);
    if (info == null)
      return NotFound();

    if (!info.Tags.Contains($"{shared.WorkspaceId}-context:{shared.Context}")) return NotFound();
    var file = await classificationClient.GetDocument(shared.Id);
    if (file == null)
      return NotFound();

    var filename = $"{info.Id}.{info.ContentType.Split('/').LastOrDefault()}";

    return File(file, info.ContentType, filename, true);
  }

  [HttpGet("share/{id}")]
  [Authorize(WisePolicies.NeedsWorkspace)]
  public async Task<IActionResult> Share(string context, string id, long? slifetime)
  {
    var sharedFileInfo = await axon.Send(new GetSharedFile()
    {
      Context = context,
      Id = id,
      SLifetime = slifetime
    });
    if (sharedFileInfo == null)
      return NotFound();
    return Ok(sharedFileInfo.Token);
  }

  [HttpGet("~/api/i/{id}/once")]
  [ApiExplorerSettings(IgnoreApi = true)]
  public async Task<IActionResult> GetOnce(string id)
  {
    try
    {
      return File(await this.GetCachedFile(id), "application/octet-stream");
    }
    finally
    {
      this.RemoveCachedFile(id);
    }
  }

  private void RemoveCachedFile(string id)
  {
    var path = Path.Combine(this.environment.ContentRootPath, "cache", id);
    if (System.IO.File.Exists(path))
      System.IO.File.Delete(path);
  }

  private async Task<byte[]> GetCachedFile(string id)
  {
    var path = Path.Combine(this.environment.ContentRootPath, "cache", id);
    if (System.IO.File.Exists(path))
      return await System.IO.File.ReadAllBytesAsync(path);
    return null;
  }

  private async Task SetCachedFile(byte[] data, string id)
  {
    var path = Path.Combine(this.environment.ContentRootPath, "cache");

    logger.LogInformation($"file cached on {path}");
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    await System.IO.File.WriteAllBytesAsync(Path.Combine(path, id), data);
  }

  private async Task SetCachedFile(Stream data, string id)
  {
    var path = Path.Combine(this.environment.ContentRootPath, "cache");

    logger.LogInformation($"file cached on {path}");
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    using (var file = new FileStream(Path.Combine(path, id), FileMode.CreateNew, FileAccess.Write))
    {
      await data.CopyToAsync(file);
    }
  }
}
