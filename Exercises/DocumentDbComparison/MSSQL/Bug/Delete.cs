using Microsoft.EntityFrameworkCore;

namespace DocumentDbComparison.MSSQL.Bug;

public static class Delete
{
    public static RouteGroupBuilder DeleteBugWithSql(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugSqlDb, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is not { } bug)
                return TypedResults.NotFound();

            bugsDb.Bugs.Remove(bug);

            await bugsDb.SaveChangesAsync(token);

            return TypedResults.Ok(bug.Id);
        };
}