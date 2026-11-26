#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using MapZilla;
using dto = Wise.Tasks.Core.Dto;


namespace Wisetown.ContextBroker.Worker;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    CreateMap<dto.Task, FIWARE.SmartDataModels.IssueTracking.IssueReporting>()
      .ForMember(i => i.Category, i => i.MapFrom(t => t.Category))
      .ForMember(i => i.Id, i => i.MapFrom(t => $"urn:wt-{t.Group.TaskType.ToLower()}:{t.Id}"))
      .ForMember(i => i.Type, i => i.MapFrom(t => t.Group.TaskType))
      .ForMember(i => i.DepartmentId, i => i.MapFrom(t => t.Group.Id))
      .ForMember(i => i.Comments, i => i.MapFrom(t => t.Description))
      .ForMember(i => i.Title, i => i.MapFrom(t => t.Title))
      .ForMember(i => i.ResolutionStatus, i => i.MapFrom(t => t.State))
      .ForMember(i => i.ObservationDateTime, i => i.MapFrom(t => t.CreationDate));
  }
}
