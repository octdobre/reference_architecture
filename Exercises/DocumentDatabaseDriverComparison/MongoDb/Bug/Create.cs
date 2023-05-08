using MongoDB.Driver;

namespace DocumentDatabaseDriverComparison.MongoDb.Bug;

public static class Create
{
    private const string PathName = "CreateBug";

    public record CreateRequest(
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder PostBugWithMongoDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName(PathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        return routeGroupBuilder;
    }

    private static readonly Func<CreateRequest, BugDocumentDb, LinkGenerator, CancellationToken, Task<IResult>> Handler
        = async (createdBug, bugsDb, linker, token) =>
        {
            var newBug = new BugDocumentDb.BugDocument(
                Guid.NewGuid(),
                createdBug.Title,
                createdBug.Description,
            createdBug.ReportTime);

            await bugsDb.BugCollection.InsertOneAsync(newBug, new InsertOneOptions(), token);

            return TypedResults.Created($"{linker.GetPathByName(PathName)}/{newBug.Id}", newBug);
        };
}