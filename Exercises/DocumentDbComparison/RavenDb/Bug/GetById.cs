namespace DocumentDbComparison.RavenDb.Bug;

public static class GetById
{
    public record GetBug(
        string Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder GetBugByIdWithRavenDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/{id:guid}", Handler)
            .WithName("GetBugById")
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugRavenDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
    {
        using var session = bugsDb.Store.OpenAsyncSession();
        var bugInDb = await session.LoadAsync<BugRavenDbRepo.BugDocument>(id.ToString(), token);

        return bugInDb is { }
            ? TypedResults.Ok(new GetBug(bugInDb.Id, bugInDb.Title, bugInDb.Description, bugInDb.ReportTime))
            : TypedResults.NotFound();
    };
}