using System.Collections.Immutable;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Wisetown.Lake.Classification.Handler.Model;

public class DB : DbContext
{
  public DbSet<Document> Documents { get; set; }
  public DbSet<DocumentTag> DocumentTags { get; set; }
  public DbSet<Trash> Trash { get; set; }

  public DB(DbContextOptions<DB> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.HasDefaultSchema("trasimeno");

    modelBuilder.Entity<DocumentTag>().HasKey(i => new { i.Id, i.Tag });
  }
}

public class SQLMigrationContext : DB
{
  public SQLMigrationContext(DbContextOptions<DB> options) : base(options)
  {
  }
}

public class SQLContextFactory : IDesignTimeDbContextFactory<SQLMigrationContext>
{
  public SQLMigrationContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<DB>();
    optionsBuilder.UseSqlServer();
    return new SQLMigrationContext(optionsBuilder.Options);
  }
}

public class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger) : DbCommandInterceptor
{
  private const int SlowQueryThreshold = 1;

  public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
    CancellationToken cancellationToken = default)
  {
    if (eventData.Duration.Seconds > SlowQueryThreshold)
      logger.LogWarning($"Slow query detected ({eventData.Duration.TotalMilliseconds} ms): {command.CommandText}");

    return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
  }
}