using MapZilla;
using Hikyaku;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wisetown.Lake.Classification.Handler.Model;
using _commands = Wisetown.Lake.Classification.Core.Command;
using _dto = Wisetown.Lake.Classification.Core.Dto;

namespace Wisetown.Lake.Classification.Handler.CQRS
{
  public class ClassificationsCommandHandler(
     DB db,
    ILogger<ClassificationsCommandHandler> logger,
    IMapper mapper)
    : IRequestHandler<_commands.Set, _dto.DocumentInfo>,
      IRequestHandler<_commands.AddClassification>,
      IRequestHandler<_commands.RemoveClassification>,
      IRequestHandler<_commands.Delete>,
      IRequestHandler<_commands.PermanentDelete>
  {
    public async Task<_dto.DocumentInfo> Handle(_commands.Set request, CancellationToken cancellationToken)
    {
      var doc = await db.Documents.Where(i => i.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

      if (doc == null)
      {
        doc = new Document()
        {
          Id = request.Id,
          IngestionDate = DateTime.Now,
          UpdateDate = DateTime.Now,
          Name = request.Name ?? request.Id,
          Size = request.Size, 
          Position = request.Position, 
          ContentType = request.ContentType
        };
        db.Documents.Add(doc);
      }
      else
      {
        doc.UpdateDate = DateTime.Now;
      }

      await db.SaveChangesAsync(cancellationToken);
      return mapper.Map<_dto.DocumentInfo>(doc);
    }

    public Task Handle(_commands.Delete request, CancellationToken cancellationToken)
    {
      logger.LogWarning("Document deletion has not been implemented. ");
      return Task.CompletedTask;
    }

    public async Task Handle(_commands.RemoveClassification request, CancellationToken cancellationToken)
    {
      var tags = await db.DocumentTags.Where(i => i.Id == request.Id).ToListAsync(cancellationToken);

      foreach (var tag in request.Tags)
      {
        var et = tags.Find(i => i.Id == request.Id && i.Tag == tag);

        if (et != null)
          db.DocumentTags.Remove(et);
      }

      await db.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(_commands.AddClassification request, CancellationToken cancellationToken)
    {
      var tags = await db.DocumentTags.Where(i => i.Id == request.Id).ToListAsync(cancellationToken);

      foreach (var tag in request.Tags)
      {
        if (tags.Exists(i => i.Id == request.Id && i.Tag == tag)) continue;
        db.DocumentTags.Add(new DocumentTag() { Id = request.Id, Tag = tag });
      }

      await db.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(_commands.PermanentDelete request, CancellationToken cancellationToken)
    {
      var tags = await db.DocumentTags.Where(i => i.Id == request.Id).ToListAsync(cancellationToken);
      var doc = await db.Documents.Where(i => i.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

      db.DocumentTags.RemoveRange(tags);
      if (doc != null) db.Documents.Remove(doc);

      await db.SaveChangesAsync(cancellationToken);
    }
  }
}
