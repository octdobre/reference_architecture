using Microsoft.EntityFrameworkCore;

namespace DocumentDbComparison.MSSQL.Bug;

public static class Update
{
    public record UpdateBug(
        string? Title,
        string? Description);

    public static RouteGroupBuilder UpdateBugWithSql(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/sql/bug/{id:guid}", Handler)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, UpdateBug, BugSqlDb, CancellationToken, Task<IResult>> Handler
        = async (id, updateBug, bugsDb, token) =>
        {
            if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is not { } bug)
                return TypedResults.NotFound();

            var updatedBug = bug with
            {
                Title = updateBug.Title ?? bug.Title,
                Description = updateBug.Description ?? bug.Description
            };

            bugsDb.Update(updatedBug);

            await bugsDb.SaveChangesAsync(token);

            return TypedResults.Ok(updatedBug);
        };
}