using Hikyaku; 
using Microsoft.EntityFrameworkCore;
using Wisetown.Lake.Classification.Handler.Model;
using _queries = Wisetown.Lake.Classification.Core.Query;
using _dto = Wisetown.Lake.Classification.Core.Dto;

namespace Wisetown.Lake.Classification.Handler.CQRS
{
  public class ClassificationsQueryHandler(DB db) : IRequestHandler<_queries.Get, _dto.DocumentInfo>,
    IRequestHandler<_queries.SearchTags, IEnumerable<string>>,
    IRequestHandler<_queries.SearchDocuments, IEnumerable<_dto.DocumentInfo>>,
    IRequestHandler<_queries.GetTrashed, IEnumerable<_dto.DocumentInfo>>,
    IRequestHandler<_queries.GetWorkspaceUsage, _dto.WorkspaceUsage>
  {
    public async Task<_dto.DocumentInfo> Handle(_queries.Get request, CancellationToken cancellationToken)
    {
      return await (from doc in db.Documents
        join tags in db.DocumentTags on
          doc.Id equals tags.Id
        where doc.Id == request.Id
        group tags by doc
        into gg
        select new _dto.DocumentInfo()
        {
          Id = gg.Key.Id,
          ContentType = gg.Key.ContentType,
          Size = gg.Key.Size,
          Position = gg.Key.Position,
          Name = gg.Key.Name,
          IngestionDate = gg.Key.IngestionDate,
          UpdateDate = gg.Key.UpdateDate,
          Stoned = gg.Key.Stoned,
          Tags = request.IncludeTags ? gg.Select(t => t.Tag).ToArray() : null
        }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> Handle(_queries.SearchTags request, CancellationToken cancellationToken)
    {
      var query = db.DocumentTags.Where(i => i.Tag.Contains(request.Pattern))
        .Select(t => t.Tag).Distinct();

      if (request.skip != null) query = query.Skip(request.skip.Value);
      if (request.take != null) query = query.Take(request.take.Value);

      return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<_dto.DocumentInfo>> Handle(_queries.SearchDocuments request, CancellationToken cancellationToken)
    {
      return await (from doc in db.Documents
        join tags in db.DocumentTags on doc.Id equals tags.Id
        where request.Exact ? tags.Tag == request.Pattern : tags.Tag.Contains(request.Pattern)
        group tags by doc
        into gg
        select new _dto.DocumentInfo()
        {
          Id = gg.Key.Id,
          ContentType = gg.Key.ContentType,
          Size = gg.Key.Size,
          Position = gg.Key.Position,
          Name = gg.Key.Name,
          IngestionDate = gg.Key.IngestionDate,
          UpdateDate = gg.Key.UpdateDate,
          Stoned = gg.Key.Stoned,
          Tags = gg.Select(t => t.Tag).ToArray()
        }).ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<_dto.DocumentInfo>> Handle(_queries.GetTrashed request, CancellationToken cancellationToken)
    {
      return Task.FromResult((new _dto.DocumentInfo[] { }).AsEnumerable());
    }

    public async Task<_dto.WorkspaceUsage> Handle(_queries.GetWorkspaceUsage request, CancellationToken cancellationToken)
    {
      var prefix = $"{request.WorkspaceId}-context:";

      var result = await (from doc in db.Documents
        join tags in db.DocumentTags on doc.Id equals tags.Id
        where tags.Tag.StartsWith(prefix) && !doc.Stoned
        select doc)
        .Distinct()
        .GroupBy(_ => 1)
        .Select(g => new _dto.WorkspaceUsage
        {
          WorkspaceId = request.WorkspaceId,
          DocumentCount = g.Count(),
          TotalSizeBytes = g.Sum(d => d.Size)
        })
        .FirstOrDefaultAsync(cancellationToken);

      return result ?? new _dto.WorkspaceUsage
      {
        WorkspaceId = request.WorkspaceId,
        DocumentCount = 0,
        TotalSizeBytes = 0
      };
    }
  }
}
