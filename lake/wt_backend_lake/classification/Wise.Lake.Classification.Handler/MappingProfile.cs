using MapZilla;

namespace Wisetown.Lake.Classification.Handler
{
  public class MappingProfile : Profile
  {
    public MappingProfile()
    {
      CreateMap<Classification.Core.Dto.DocumentInfo, Model.Document>().ReverseMap();
    }
  }
}
