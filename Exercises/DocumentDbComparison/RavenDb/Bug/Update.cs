namespace DocumentDbComparison.RavenDb.Bug;

public static class Update
{
    public record UpdateBug(
        string? Title,
        string? Description);

    public static RouteGroupBuilder UpdateBugWithRavenDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/{id:guid}", Handler)
            .WithName("UpdateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, UpdateBug, BugRavenDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, updateBug, bugsDb, token) =>
        {
            using var session = bugsDb.Store.OpenAsyncSession();
            if (await session.LoadAsync<BugRavenDbRepo.BugDocument>(id.ToString()) is { } bugDocument)
            {
                //Partial update of a document
                if (updateBug.Title is { })
                {
                    session.Advanced.Patch<BugRavenDbRepo.BugDocument, string>(
                        id.ToString(),
                        x => x.Title,
                        updateBug.Title);
                }

                if (updateBug.Description is { })
                {
                    session.Advanced.Patch<BugRavenDbRepo.BugDocument, string>(
                        id.ToString(),
                        x => x.Description,
                        updateBug.Description);
                }

                /* updating by replacing
                session.Advanced.Evict(bugDocument);
                await session.StoreAsync(bugDocument
                    with
                {
                    Title = updateBug.Title ?? bugDocument.Title,
                    Description = updateBug.Description ?? bugDocument.Description
                }, id.ToString(), token);
                */

                await session.SaveChangesAsync(token);

                return TypedResults.Ok(bugDocument
                    with
                {
                    Title = updateBug.Title ?? bugDocument.Title,
                    Description = updateBug.Description ?? bugDocument.Description
                });
            }
            else
                return TypedResults.NotFound();
        };
}