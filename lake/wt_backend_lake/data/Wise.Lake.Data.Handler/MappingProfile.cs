#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using MapZilla;

using classify = Wisetown.Lake.Classification.Core;

namespace Wisetown.Lake.Data.Handler
{
  public class MappingProfile : Profile
  {
    public MappingProfile()
    {
      this.CreateMap<classify.Dto.DocumentInfo, Core.Dto.DocumentInfo>();
      this.CreateMap<Core.Dto.Document, Model.DataModel>().ReverseMap();
      this.CreateMap<Core.Command.Set, Model.DataModel>();

      this.CreateMap<Wisetown.Lake.Core.Commands.StoreMessage, Wisetown.Lake.Core.Commands.StoreNotification>().ReverseMap();
      this.CreateMap<Wisetown.Lake.Core.Commands.StoneMessage, Wisetown.Lake.Core.Commands.StoneNotification>().ReverseMap();
    }
  }
}
