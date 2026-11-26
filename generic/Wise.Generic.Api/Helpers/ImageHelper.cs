using System;
using System.IO;
using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public static class ImageHelper
{
  public static string GetUniqueFileName(byte[] filedata, string filename)
  {
    var fileformat = Path.GetExtension(filename);

    if (IsImage(fileformat))
      return GetUniqueFileName(filedata, Image.DetectFormat(filedata));

    if (fileformat.StartsWith("."))
      fileformat = fileformat.Replace(".", string.Empty);

    var hasher = MD5.Create();
    return string.Format("{0}.{1}", Convert.ToBase64String(hasher.ComputeHash(filedata)), fileformat.ToString()).Replace('/', '_').Replace("\"", "_").Replace("+", "_P_");
  }

  public static string GetUniqueFileName(byte[] image, IImageFormat format)
  {
    var hasher = MD5.Create();
    return string.Format("{0}.{1}", Convert.ToBase64String(hasher.ComputeHash(image)), format.ToString()).Replace('/', '_').Replace("\"", "_").Replace("+", "_P_");
  }

  public static bool IsImage(string fileformat)
  {
    if (fileformat.StartsWith("."))
      fileformat = fileformat.Replace(".", string.Empty);

    if (fileformat == "jpg")
      fileformat = "jpeg";

    switch (fileformat.ToLower())
    {
      case "bmp":
      case "emf":
      case "exif":
      case "gif":
      case "icon":
      case "jpg":
      case "jpeg":
      case "png":
      case "tiff":
      case "wmf":
        return true;
    }
    return false;
  }

  public static void ResizeImage(this Image image, int imageWidth, int imageHeight, bool stretch = false, bool crop = false, string cropfrom = "")
  {
    if (imageWidth == 0 && imageHeight == 0)
    {
      image = new Image<Rgba32>(0, 0);
      return;
    }

    if (imageWidth == 0 || imageHeight == 0)
      crop = false;

    if (crop)
    {
      var cropRect = CalcCropMargin(imageWidth, imageHeight, image.Width, image.Height);
      switch (cropfrom.ToLower())
      {
        case "top":
          cropRect = new Rectangle(0, 0, cropRect.Width, cropRect.Height);
          break;
        default:
          break;
      }
      image.Mutate(f => f.Crop(cropRect));
      return;
    }

    // If we can stretch the image, we can directly call Resize mutation.
    if (stretch == true)
    {
      image.Mutate(f => f.Resize(imageWidth, imageHeight));
      return;
    }

    // If you pass 0 as any of the values for width and height dimensions then ImageSharp
    //  will automatically determine the correct opposite dimensions size to preserve the original aspect ratio.
    if (imageWidth == 0 || imageHeight == 0)
    {
      image.Mutate(f => f.Resize(imageWidth, imageHeight));
      return;
    }

    // Here we must choose wich one (of w or h) to use to resize the image proportionally (stretch = false)
    double px = (image.Width * imageHeight) / image.Height;
    double py = (image.Height * imageWidth) / image.Width;

    if (px < imageWidth)
      px = imageWidth - px;

    if (py < imageHeight)
      py = imageHeight - py;

    if (px > py)
    {
      image.Mutate(f => f.Resize(imageWidth, 0));
    }
    else
    {
      image.Mutate(f => f.Resize(0, imageHeight));
    }
  }

  private static Rectangle CalcCropMargin(int finalw, int finalh, int imgw, int imgh)
  {
    var py = (double)finalh / (double)imgh;
    var px = (double)finalw / (double)imgw;

    var sfactor = Math.Max(px, py);

    var boxx = finalw / sfactor;
    var boxy = finalh / sfactor;

    var xdelta = imgw - boxx;
    var ydelta = imgh - boxy;

    return new Rectangle((int)(xdelta / 2), (int)(ydelta / 2), (int)boxx, (int)boxy);
  }

}
