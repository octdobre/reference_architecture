using Couchbase.KeyValue;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DocumentDbComparison.Couchbase.Bug;

public static class Create
{
    private const string PathName = "CreateBug";

    public record CreateRequest(
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder PostBugWithCouchbaseDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName(PathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        return routeGroupBuilder;
    }

    private static readonly Func<CreateRequest, BugCouchbaseDbRepo, LinkGenerator, CancellationToken, Task<IResult>> Handler
        = async (createdBug, bugsDb, linker, token) =>
        {
            var insertOptions = new InsertOptions();
            insertOptions.CancellationToken(token);

            var newBug = new BugCouchbaseDbRepo.BugDocument(
                Guid.NewGuid(),
                createdBug.Title,
                createdBug.Description,
            createdBug.ReportTime);

            var mutationResult = await bugsDb.BugCollection.InsertAsync(newBug.Id.ToString(), newBug, insertOptions);

            return TypedResults.Created($"{linker.GetPathByName(PathName)}/{newBug.Id}", newBug);
        };
}