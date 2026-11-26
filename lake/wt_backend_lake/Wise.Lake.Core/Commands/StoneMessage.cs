using Hikyaku;

namespace Wisetown.Lake.Core.Commands;

public class StoneMessage : IRequest
{
  
  public string ID { get; set; }
  public string __messageKey { get; } = "stone";
}

public class StoneNotification : IRequest, INotification
{
  
  public string ID { get; set; }
  public string __messageKey { get; } = "stone";
}
