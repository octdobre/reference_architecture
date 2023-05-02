using Squash.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Squash.Api.Features.Bug;

public static class Delete
{
    public static void SetupDeleteBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static readonly Func<Guid, BugDb, CancellationToken, Task<IResult>> Handler = async (id, bugsDb, token) =>
    {
        if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is { } bug)
        {
            bugsDb.Bugs.Remove(bug);

            await bugsDb.SaveChangesAsync(token);

            return TypedResults.Ok(bug.Id);
        }
        else
            return TypedResults.NotFound();
    };
}