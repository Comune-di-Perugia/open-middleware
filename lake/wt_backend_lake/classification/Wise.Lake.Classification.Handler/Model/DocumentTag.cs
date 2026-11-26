using System.ComponentModel.DataAnnotations;

namespace Wisetown.Lake.Classification.Handler.Model;

public class DocumentTag
{
  [StringLength((256))] public string Id { get; set; }
  [StringLength(256)] public string Tag { get; set; }
}