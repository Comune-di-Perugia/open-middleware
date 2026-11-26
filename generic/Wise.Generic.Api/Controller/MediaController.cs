using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Generic.api.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using Wise.Core;
using Wise.Web;

namespace Generic.api.Controller
{
  [ApiController]
  [Route("~/generic/{context}/")]
  [ControllerGroupBy("generic")]
  [ControllerGroupBy("generic-media")]
  public class MediaController : ControllerBase
  {
    private readonly IClassificationClient _classificationClient;

    public MediaController(IClassificationClient classificationClient)
    {
      this._classificationClient = classificationClient;
    }
    
    [HttpGet]
    [Route("thumb/{filename}")]
    [Authorize(WisePolicies.NeedsWorkspace)]
    public async Task<IActionResult> Thumb(string context, string filename, string format = "jpg", int width = 240,
      int height = 180, int quality = 75)
    {
      return await Resize(context, filename, format, width, height, false, false, "", quality);
    }

    [HttpGet]
    [Route("image/{filename}")]
    [Authorize(WisePolicies.NeedsWorkspace)]
    public async Task<IActionResult> GetImage(string context, string filename)
    {
      try
      {
        var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));

        var info = await _classificationClient.Info(filename);
        if (info == null) return NotFound();
        if (!info.Tags.Contains($"{wspId}-context:{context}")) return NotFound();

        var file = await _classificationClient.GetDocument(filename);
        if (file == null) return NotFound();

        return File(file, info.ContentType);
      }
      catch
      {
        return NotFound();
      }
    }

    [HttpGet]
    [Route("image/{filename}/resize")]
    [Authorize(WisePolicies.NeedsWorkspace)]
    [Route("image/{filename}.{format}/resize")]
    public async Task<IActionResult> Resize(string context, string filename, string format = "jpg", int width = 700,
      int height = 700, bool stretch = false,
      bool crop = false, string cropfrom = "", int quality = 95)
    {
      IImageFormat imgformat = SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance;
      var wspId = Guid.Parse(this.User.FindFirstValue(WiseClaims.currentWorkspace));

      switch (format.ToLower())
      {
        case "png":
          imgformat = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
          break;
        case "jpg":
        case "jpeg":
          imgformat = SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance;
          break;
        default: return BadRequest("Invalid output Image File Format. Accepted : jpeg, jpg, png");
      }

      try
      {
        var info = await _classificationClient.Info(filename);
        if (info == null)
          return NotFound();
        if (!info.Tags.Contains($"{wspId}-context:{context}"))
          return NotFound();

        var file = await _classificationClient.GetDocument(filename);
        if (file == null)
          return NotFound();

        using var image = Image.Load(file); //, out IImageFormat imgFormat)) // if we need to know original file format
        image.ResizeImage(width, height, stretch, crop, cropfrom);

        using var stream = new System.IO.MemoryStream();
        image.Save(stream, imgformat);

        return File(stream.GetBuffer(), imgformat.MimeTypes.First());
      }
      catch
      {
        return NotFound();
      }
    }
  }
}
