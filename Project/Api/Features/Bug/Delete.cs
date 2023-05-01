using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Bug;

public static class Delete
{
    public static void SetupDeleteBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi();
    }

    private static readonly Func<Guid, BugDb, CancellationToken, Task<IResult>> Handler = async (id, bugsDb, token) =>
    {
        if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is { } bug)
        {
            bugsDb.Bugs.Remove(bug);

            await bugsDb.SaveChangesAsync(token);

            return TypedResults.Ok(new
            {
                DeletedId = bug.Id
            });
        }
        else
            return TypedResults.NotFound(new
            {
                NotFoundId = id,
                Deleted = false
            });
    };
}