using MongoDB.Driver;

namespace DocumentDatabaseDriverComparison.MongoDb.Bug;

public static class GetById
{
    public record GetBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder GetBugByIdWithMongoDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/{id:guid}", Handler)
            .WithName("GetBugById")
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugDocumentDb, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
    {
        var filter = Builders<BugDocumentDb.BugDocument>.Filter.Eq(e => e.Id, id);

        //example of a projection
        var projection = Builders<BugDocumentDb.BugDocument>.Projection
            .Include(nameof(BugDocumentDb.BugDocument.Id))
            .Include(nameof(BugDocumentDb.BugDocument.Title))
            .Include(nameof(BugDocumentDb.BugDocument.Description))
            .Include(nameof(BugDocumentDb.BugDocument.ReportTime));

        //projection protects new fields from appear in the get result
        return await bugsDb.BugCollection.Find(filter).Project<GetBug>(projection).FirstOrDefaultAsync(token) is { } bug
            ? TypedResults.Ok(bug)
            : TypedResults.NotFound();
    };
}