using MongoDB.Driver;

namespace DocumentDbComparison.MongoDb.Bug;

public static class Update
{
    public record UpdateBug(
        string? Title,
        string? Description);

    public static RouteGroupBuilder UpdateBugWithMongoDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/{id:guid}", Handler)
            .WithName("UpdateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, UpdateBug, BugMongoDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, updateBug, bugsDb, token) =>
    {
        var filter = Builders<BugMongoDbRepo.BugDocument>.Filter.Eq(e => e.Id, id);

        if (await bugsDb.BugCollection.Find(filter).FirstOrDefaultAsync(token) is { } bug)
        {
            var updatedBug = bug with { Title = updateBug.Title!, Description = updateBug.Description! };
            await bugsDb.BugCollection.ReplaceOneAsync(filter, updatedBug);

            return TypedResults.Ok(updatedBug);
        }
        else
            return TypedResults.NotFound();
    };
}