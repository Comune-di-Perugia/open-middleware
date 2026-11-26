using System.Text;
using MapZilla;
using Hikyaku;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Wisetown.Lake.Data.Handler.CQRS;

using lakeCommands = Wisetown.Lake.Core.Commands;

public class LakeCommands(IHikyaku axon, ILogger<LakeCommands> logger, IMapper mapper, IConfiguration configuration)
  :
    IRequestHandler<lakeCommands.StoneMessage>,
    IRequestHandler<lakeCommands.StoreMessage>
{
  public async Task Handle(lakeCommands.StoneMessage request, CancellationToken cancellationToken)
  {
    await axon.Publish(mapper.Map<lakeCommands.StoneNotification>(request));
    await axon.Send(new Data.Core.Command.Delete() { Id = request.ID });
  }

  public async Task Handle(lakeCommands.StoreMessage request, CancellationToken cancellationToken)
  {
    byte[] buffer = null;

    logger.LogInformation($"Store Message request:{request.BodyType} {request.DocumentUri}");

    switch (request.BodyType)
    {
      case lakeCommands.DocumentBodyEnum.Embedded:
        buffer = Encoding.UTF8.GetBytes(request.DocumentBody);
        break;
      case lakeCommands.DocumentBodyEnum.ExternalUri:
        if (!string.IsNullOrWhiteSpace(request.DocumentUri))
          buffer = await new HttpClient().GetByteArrayAsync(request.DocumentUri);
        break;
    }


    if (buffer == null)
      return;

    await axon.Send(new Wisetown.Lake.Data.Core.Command.Set()
    {
      ContentType = request.ContentType,
      Data = buffer,
      Id = request.ID,
      Name = request.DocumentName
    });

    if (!string.IsNullOrWhiteSpace(request.RootClassification))
      await axon.Send(new Wisetown.Lake.Classification.Core.Command.AddClassification()
      {
        Id = request.ID,
        Tags = new string[]
        {
            request.RootClassification,
            $"ContentType:{request.ContentType}",
            $"Owner:{request.UserId}"
        }
      });

    await axon.Publish(new lakeCommands.StoreNotification()
    {
      ContentType = request.ContentType,
      BodyType = request.BodyType,
      RootClassification = request.RootClassification,
      WorkspaceId = request.WorkspaceId,
      UserId = request.UserId,
      Context = request.Context,
      DocumentName = request.DocumentName,
      DocumentBody = request.DocumentBody,
      DocumentUri = $"{configuration.GetValue<string>("middleware:IndexingServiceUri")}/api/data/{request.ID}"
    });
    return;
  }
}
