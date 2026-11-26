#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.


using Hikyaku;
using MapZilla;
using query = Wisetown.Lake.Data.Core.Query;
using dto = Wisetown.Lake.Data.Core.Dto;
using classify = Wisetown.Lake.Classification.Core;

namespace Wisetown.Lake.Data.Handler.CQRS
{
  public class DataQueryHandler( /*MongoService mongo,*/Services.IStorageService storage, IMapper mapper, IHikyaku axon)
    : IRequestHandler<query.Get, dto.Document>,
      IRequestHandler<query.Exists, bool>,
      IRequestHandler<query.GetInfo, dto.DocumentInfo>
  {
    public async Task<dto.Document> Handle(query.Get request, CancellationToken cancellationToken)
    {
      var info = await axon.Send(new classify.Query.Get() { Id = request.Id });
      if (info == null) return null;

      switch (info.Position)
      {
        case "store":
        {
          return new dto.Document
          {
            Id = request.Id,
            ContentType = info.ContentType,
            Data = await storage.Get(request.Id, info.ContentType)
          };
        }
      }

      return null;
    }

    public async Task<bool> Handle(query.Exists request, CancellationToken cancellationToken)
    {
      var info = await axon.Send(new classify.Query.Get() { Id = request.Id });
      if (info != null) return true;
      return false;
    }

    public async Task<dto.DocumentInfo> Handle(query.GetInfo request, CancellationToken cancellationToken)
    {
      var info = await axon.Send(new classify.Query.Get() { Id = request.Id });
      return mapper.Map<dto.DocumentInfo>(info);
    }
  }
}
