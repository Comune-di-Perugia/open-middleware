using System;
using System.Threading;
using System.Threading.Tasks;
using Hikyaku;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Wise.Generic.Core.Dtos;

namespace Wise.Generic.Handlers.CQRS;

using Core = Wise.Generic.Core;

public class ShareQueryHandler(IHikyaku axon) :
  IRequestHandler<Core.Queries.GetSharedFile, SharedFile>
{
  public async Task<SharedFile> Handle(Core.Queries.GetSharedFile request, CancellationToken cancellationToken)
  {
    var info = await axon.Send(new Wisetown.Lake.Classification.Core.Query.Get()
    {
      Id = request.Id,
      IncludeTags = true
    });
    if (info == null)
      return null;

    if (!info.Tags.Contains($"{request.WorkspaceId}-context:{request.Context}"))
      return null;

    var data = new SharedData()
    {
      Id = request.Id,
      WorkspaceId = request.WorkspaceId,
      Context = request.Context,
      ExpirationDate = request.SLifetime != null ? DateTime.Now.AddSeconds(request.SLifetime.Value) : null
    };
    var jsondata = JsonConvert.SerializeObject(data);

    return new SharedFile()
    {
      Token = Base64UrlTextEncoder.Encode(Core.CryptoUtils.EncryptData(jsondata))
    };
  }
}
