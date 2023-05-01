using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Bug;

public static class Update
{
    public class UpdateBug
    {
        public string? Title { get; set; }

        public string? Description { get; set; }
    }

    public static void SetupUpdateBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/{id:guid}", Handler)
            .WithName("UpdateBug")
            .WithOpenApi();
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

            await bugsDb.Bugs.AddAsync(updatedBug, token);
            await bugsDb.SaveChangesAsync(token);

            return TypedResults.Ok(updatedBug);
        }
        else
            return TypedResults.NotFound(
                new
                {
                    NotFoundId = id,
                    Updated = false
                });
    };
}