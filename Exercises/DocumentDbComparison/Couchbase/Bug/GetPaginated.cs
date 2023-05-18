using Couchbase.Query;

namespace DocumentDbComparison.Couchbase.Bug;

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

    record CountResult(int CountBugs);

    public static RouteGroupBuilder GetPaginatedBugsWithCouchbaseDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/", Handler)
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<int?, int?, bool?, BugCouchbaseDbRepo, CancellationToken, Task<IResult>> Handler
        = async (pageNumber, pageSize, sortByTitle, bugsDb, token) =>
        {
            //default values
            pageNumber ??= 1;

            pageSize = pageSize is { }
                ? pageSize > 10 || pageSize < 0 ? 10
                : pageSize : 10;

            sortByTitle ??= true;
            var skip = (pageNumber - 1) * pageSize;

            var sortDirectionString = sortByTitle.Value ? "ASC" : "DESC";

            var queryOptions = new QueryOptions();
            queryOptions.CancellationToken(token);

            var countQuery =
                $" SELECT Count(*) as CountBugs FROM {bugsDb.BucketName}.{bugsDb.ScopeName}.{bugsDb.CollectionName}";
            var countQueryResult = await bugsDb.BugCluster.QueryAsync<CountResult>(countQuery, queryOptions);
            var countResult = await countQueryResult.Rows.ToListAsync<CountResult>();

            var query =
                $" SELECT b.* FROM {bugsDb.BucketName}.{bugsDb.ScopeName}.{bugsDb.CollectionName} b" +
                $" ORDER BY title {sortDirectionString} " +
                $" LIMIT {pageSize} " +
                $" OFFSET {skip} ";

            var results = await bugsDb.BugCluster.QueryAsync<PagedBug>(query, queryOptions);
            var items = await results.Rows.ToListAsync<PagedBug>();

            return items.Any()
                ? TypedResults.Ok(new Page(items.Select(b => new PagedBug(b.Id, b.Title, b.Description, b.ReportTime)), items.Count(), (int)countResult.First().CountBugs))
                : TypedResults.NoContent();
        };
}