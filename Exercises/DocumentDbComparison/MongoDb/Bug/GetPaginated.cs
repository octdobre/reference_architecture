using MongoDB.Driver;

namespace DocumentDbComparison.MongoDb.Bug;

public static class GetPaginated
{
    public record PagedBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public record Page(
        IEnumerable<PagedBug> Bugs,
        int Displayed,
        int Total);

    public static RouteGroupBuilder GetPaginatedBugsWithMongoDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/", Handler)
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<int?, int?, bool?, BugMongoDbRepo, CancellationToken, Task<IResult>> Handler
        = async (pageNumber, pageSize, sortByTitle, bugsDb, token) =>
        {
            //default values
            pageNumber ??= 1;

            pageSize = pageSize is { }
                ? pageSize > 10 || pageSize < 0 ? 10
                : pageSize : 10;

            sortByTitle ??= true;
            var skip = (pageNumber - 1) * pageSize;

            var queryExpression = bugsDb.BugCollection.Find(_ => true).Skip(skip).Limit(pageSize);
            queryExpression = sortByTitle.Value
                    ? queryExpression.SortBy(b => b.Title)
                    : queryExpression.SortByDescending(b => b.Title);

            var items = await queryExpression.ToListAsync(token);

            var filter = Builders<BugMongoDbRepo.BugDocument>.Filter.Empty;
            var count = await bugsDb.BugCollection.CountDocumentsAsync(filter);

            return items.Any()
                ? TypedResults.Ok(new Page(items.Select(b => new PagedBug(b.Id, b.Title, b.Description, b.ReportTime)), items.Count, (int)count))
                : TypedResults.NoContent();
        };
}