using Squash.Api.Infrastructure;
using MongoDB.Driver;

namespace Squash.Api.Features.Bug;

public static class Delete
{
    public static void SetupDeleteBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static readonly Func<Guid, BugDocumentDb, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            var filter = Builders<BugDocumentDb.BugDocument>.Filter.Eq(e => e.Id, id);

            var deleteResult = await bugsDb.BugCollection.DeleteOneAsync(filter, token);

            if (deleteResult.DeletedCount == 1)
            {
                return TypedResults.Ok(id);
            }
            else if (deleteResult.DeletedCount == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.BadRequest();
        };
}