using Microsoft.EntityFrameworkCore;

namespace Squash.Api.Infrastructure;

[Obsolete]
public class BugInMemoryDb : DbContext
{
    public record BugDocument(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public BugInMemoryDb(DbContextOptions<BugInMemoryDb> options)
        : base(options) { }

    public DbSet<BugDocument> Bugs => Set<BugDocument>();
}