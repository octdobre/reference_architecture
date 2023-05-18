using Raven.Client.Documents;

namespace DocumentDbComparison.RavenDb.Bug;

public static class GetPaginated
{
    public record PagedBug(
        string Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public record Page(
        IEnumerable<PagedBug> Bugs,
        int Displayed,
        int Total);

    public static RouteGroupBuilder GetPaginatedBugsWithRavenDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/", Handler)
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<int?, int?, bool?, BugRavenDbRepo, CancellationToken, Task<IResult>> Handler
        = async (pageNumber, pageSize, sortByTitle, bugsDb, token) =>
        {
            //default values
            pageNumber ??= 1;

            pageSize = pageSize is { }
                ? pageSize > 10 || pageSize < 0 ? 10
                : pageSize : 10;

            sortByTitle ??= true;
            var skip = (pageNumber - 1) * pageSize;

            using var session = bugsDb.Store.OpenAsyncSession();

            IQueryable<BugRavenDbRepo.BugDocument> query = session.Query<BugRavenDbRepo.BugDocument>(collectionName:"Bugs");

            var count = await query.CountAsync(token);

            query = sortByTitle.Value
                ? query.OrderBy(bug => bug.Title)
                : query.OrderByDescending(bug => bug.Title);

            var items = await query.Skip(skip.Value).Take(pageSize.Value).ToListAsync(token);

            return items.Any()
                ? TypedResults.Ok(new Page(items.Select(b => new PagedBug(b.Id, b.Title, b.Description, b.ReportTime)), items.Count, count))
                : TypedResults.NoContent();
        };
}