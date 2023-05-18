using Microsoft.EntityFrameworkCore;

namespace DocumentDbComparison.MSSQL.Bug;

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

    public static RouteGroupBuilder GetPaginatedBugWithSql(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/sql/bug/", Handler)
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<int?, int?, bool?, BugSqlDb, CancellationToken, Task<IResult>> Handler
        = async (pageNumber, pageSize, sortByTitle, bugsDb, token) =>
        {
            IQueryable<BugSqlDb.BugRecord> query = bugsDb.Bugs;

            var totalItems = await query.CountAsync(token);

            pageNumber ??= 1;
            pageSize = pageSize is { }
                ? pageSize > 10 || pageSize < 0 ? 10
                    : pageSize : 10;
            sortByTitle ??= true;
            var skip = (pageNumber - 1) * pageSize;

            query = sortByTitle.Value
                    ? query.OrderBy(b => b.Title)
                    : query.OrderByDescending(b => b.Title);

            var items = await query.Skip(skip.Value).Take(pageSize.Value).ToListAsync(token);

            return items.Any()
                ? TypedResults.Ok(new Page(items.Select(b => new PagedBug(b.Id, b.Title, b.Description, b.ReportTime)), items.Count, totalItems))
                : TypedResults.NoContent();
        };
}