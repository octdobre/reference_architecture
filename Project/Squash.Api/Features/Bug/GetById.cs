using MongoDB.Driver;
using Squash.Api.Infrastructure;

namespace Squash.Api.Features.Bug;

public static class GetById
{
    public record GetBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static void SetupGetByIdBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/{id:guid}", Handler)
            .WithName("GetBugById")
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static readonly Func<Guid, BugDocumentDb, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
    {
        var filter = Builders<BugDocumentDb.BugDocument>.Filter.Eq(e => e.Id, id);

        return await bugsDb.BugCollection.Find(filter).FirstOrDefaultAsync(token) is { } bug
            ? TypedResults.Ok(
                new GetBug(
                    Id: bug.Id,
                    Title: bug.
                    Title, Description:
                    bug.Description,
                    ReportTime: bug.ReportTime))
            : TypedResults.NotFound();
    };
}