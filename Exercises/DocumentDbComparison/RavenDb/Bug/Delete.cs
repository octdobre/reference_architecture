namespace DocumentDbComparison.RavenDb.Bug;

public static class Delete
{
    public static RouteGroupBuilder DeleteBugWithRavenDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugRavenDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            using var session = bugsDb.Store.OpenAsyncSession();
            session.Delete(id.ToString());
            await session.SaveChangesAsync(token);

            return TypedResults.Ok(id);
        };
}