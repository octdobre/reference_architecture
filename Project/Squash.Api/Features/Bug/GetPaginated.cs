using MongoDB.Driver;
using Squash.Api.Infrastructure;

namespace Squash.Api.Features.Bug;

public static class GetPaginated
{
    public record PagedBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public record Page(
        IEnumerable<PagedBug> bugs,
        int total);

    public static void SetupGetBugsPaginated(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/", Handler)
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static readonly Func<int?, int?, bool?, BugDocumentDb, CancellationToken, Task<IResult>> Handler
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

            var filter = Builders<BugDocumentDb.BugDocument>.Filter.Empty;
            var count = await bugsDb.BugCollection.CountDocumentsAsync(filter);

            return items.Any()
                ? TypedResults.Ok(new Page(items.Select(b => new PagedBug(b.Id, b.Title, b.Description, b.ReportTime)), (int)count))
                : TypedResults.NoContent();
        };
}