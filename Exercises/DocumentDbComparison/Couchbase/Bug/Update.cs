using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DocumentDbComparison.Couchbase.Bug;

public static class Update
{
    public record UpdateBug(
        string? Title,
        string? Description);

    public record Bug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder UpdateBugWithCouchbaseDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/{id:guid}", Handler)
            .WithName("UpdateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, UpdateBug, BugCouchbaseDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, updateBug, bugsDb, token) =>
        {
            var getOptions = new GetOptions();
            getOptions.CancellationToken(token);

            try
            {
                var getResult = await bugsDb.BugCollection.GetAsync(id.ToString(), getOptions);
                var bugToUpdate = getResult.ContentAs<Bug>()!;

                if (updateBug.Title is { })
                {
                    bugToUpdate = bugToUpdate with { Title = updateBug.Title };
                }

                if (updateBug.Description is { })
                {
                    bugToUpdate = bugToUpdate with { Description = updateBug.Description };
                }

                var replaceOptions = new ReplaceOptions();
                replaceOptions.CancellationToken(token);

                await bugsDb.BugCollection.ReplaceAsync(id.ToString(), bugToUpdate, replaceOptions);

                return TypedResults.Ok(bugToUpdate);
            }
            catch (DocumentNotFoundException)
            {
                return TypedResults.NotFound();
            }
        };
}