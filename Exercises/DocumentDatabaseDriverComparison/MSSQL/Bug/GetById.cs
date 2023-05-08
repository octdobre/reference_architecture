using Microsoft.EntityFrameworkCore;

namespace DocumentDatabaseDriverComparison.MSSQL.Bug;

public static class GetById
{
    public record GetBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder GetByIdBugWithSql(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/sql/bug/{id:guid}", Handler)
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugSqlDb, CancellationToken, Task<IResult>> Handler = async (id, bugsDb, token) =>
    {
        return await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is { } bug
            ? TypedResults.Ok(
                new GetBug(
                    Id: bug.Id,
                    Title: bug.
                        Title, Description:
                    bug.Description,
                    ReportTime: bug.ReportTime))
            : TypedResults.NotFound();
    };
}
