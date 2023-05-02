using Squash.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Squash.Api.Features.Bug;

public static class Update
{
    public record UpdateBug(
        string? Title,
        string? Description);

    public static void SetupUpdateBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/{id:guid}", Handler)
            .WithName("UpdateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static readonly Func<Guid, UpdateBug, BugDb, CancellationToken, Task<IResult>> Handler = async (id, updateBug, bugsDb, token) =>
    {
        if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is { } bug)
        {
            bugsDb.Bugs.Remove(bug);

            var updatedBug = bug with
            {
                Title = updateBug.Title ?? bug.Title,
                Description = updateBug.Description ?? bug.Description
            };

            bugsDb.Bugs.Add(updatedBug);
            await bugsDb.SaveChangesAsync(token);

            return TypedResults.Ok(updatedBug);
        }
        else
            return TypedResults.NotFound();
    };
}