namespace DocumentDbComparison.RavenDb.Bug;

public static class Create
{
    private const string PathName = "CreateBug";

    public record CreateRequest(
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder PostBugWithRavenDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName(PathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        return routeGroupBuilder;
    }

    private static readonly Func<CreateRequest, BugRavenDbRepo, LinkGenerator, CancellationToken, Task<IResult>> Handler
        = async (createdBug, bugsDb, linker, token) =>
        {
            var newBug = new BugRavenDbRepo.BugDocument(
                Guid.NewGuid().ToString(),
                createdBug.Title,
                createdBug.Description,
            createdBug.ReportTime);

            using (var session = bugsDb.Store.OpenAsyncSession())
            {
                await session.StoreAsync(newBug, newBug.Id, token);
                session.Advanced.GetMetadataFor(newBug)["@collection"] = "Bugs";
                await session.SaveChangesAsync(token);
            }

            return TypedResults.Created($"{linker.GetPathByName(PathName)}/{newBug.Id}", newBug);
        };
}