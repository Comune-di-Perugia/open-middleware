using System.ComponentModel.DataAnnotations;

namespace Wisetown.Lake.Classification.Handler.Model;

public class Trash
{
  [Key, StringLength(256)] public string Id { get; set; }
  public DateTime Date { get; set; }
  
}