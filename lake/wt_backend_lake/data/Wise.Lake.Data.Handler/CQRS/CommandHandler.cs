#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using MapZilla;
using Hikyaku;
using dto = Wisetown.Lake.Data.Core.Dto;
using commands = Wisetown.Lake.Data.Core.Command;
using classify = Wisetown.Lake.Classification.Core;

namespace Wisetown.Lake.Data.Handler.CQRS
{
  public class DataCommandHandler(Services.IStorageService storage, IMapper mapper, IHikyaku axon)
    : IRequestHandler<commands.Set, dto.DocumentInfo>, IRequestHandler<commands.Delete>, IRequestHandler<commands.UnSet>
  {
    public async Task<dto.DocumentInfo> Handle(commands.Set request, CancellationToken cancellationToken)
    {
      var model = mapper.Map<Model.DataModel>(request);

      string position = null;
      switch (request.ContentType)
      {
        // case "application/json":
        // case "text/json":

        default:
          await storage.Set(model, request.ContentType);
          position = "store";
          break;
      }


      {
        var result = await axon.Send(new classify.Command.Set()
        {
          Id = request.Id,
          Position = position,
          Name = request.Name,
          ContentType = request.ContentType,
          Size = request.Data.LongLength
        });

        return mapper.Map<dto.DocumentInfo>(result);
      }
    }

    public async Task Handle(commands.Delete request, CancellationToken cancellationToken)
    {
      // Let's do soft deletion...
      await axon.Send(new classify.Command.Delete() { Id = request.Id });

      return;
    }

    public async Task Handle(commands.UnSet request, CancellationToken cancellationToken)
    {
      // Permanetnly delete item from storage
      await storage.Delete(request.Id, request.ContentType);
      return;
    }
  }
}
