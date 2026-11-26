using Hikyaku;
using System;
namespace Wisetown.Lake.Core.Commands;

public class StoreMessage : IRequest
{
  public string ID { get; set; }
  public string __messageKey { get; } = "store";
  public string DocumentName { get; set; }
  public string ContentType { get; set; }
  public string Context { get; set; }
  public string RootClassification { get; set; }
  public DocumentBodyEnum BodyType { get; set; } = DocumentBodyEnum.Embedded;
  public string DocumentBody { get; set; }
  public string DocumentUri { get; set; }
  public bool NotifyToMiddleware { get; set; }
  public string NotificationTopic { get; set; }
  public Guid WorkspaceId { get; set; }
  public string UserId { get; set; }
}

public class StoreNotification : INotification
{
  public string ID { get; set; }
  public string __messageKey { get; } = "store";
  public string DocumentName { get; set; }
  public string ContentType { get; set; }
  public string Context { get; set; }
  public string RootClassification { get; set; }
  public DocumentBodyEnum BodyType { get; set; } = DocumentBodyEnum.Embedded;
  public string DocumentBody { get; set; }
  public string DocumentUri { get; set; }
  public bool NotifyToMiddleware { get; set; }
  public string NotificationTopic { get; set; }
  public Guid WorkspaceId { get; set; }
  public string UserId { get; set; }
}

public enum DocumentBodyEnum
{
  Embedded = 0,
  ExternalUri = 1
}
