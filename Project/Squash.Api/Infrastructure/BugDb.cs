using Microsoft.EntityFrameworkCore;

namespace Squash.Api.Infrastructure;

public class BugDb : DbContext
{
    public record BugDocument(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public BugDb(DbContextOptions<BugDb> options)
        : base(options) { }

    public DbSet<BugDocument> Bugs => Set<BugDocument>();
}