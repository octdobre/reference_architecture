using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DocumentDbComparison.Couchbase.Bug;

public static class Delete
{
    public static RouteGroupBuilder DeleteBugWithCouchbaseDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugCouchbaseDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            var removeOptions = new RemoveOptions();
            removeOptions.CancellationToken(token);

            try
            {
                await bugsDb.BugCollection.RemoveAsync(id.ToString(), removeOptions);
                return TypedResults.Ok(id);
            }
            catch(DocumentNotFoundException)
            {
                return TypedResults.NotFound();
            }
        };
}