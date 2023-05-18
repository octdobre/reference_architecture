using Couchbase.Core.Exceptions.KeyValue;
using GetOptions = Couchbase.KeyValue.GetOptions;

namespace DocumentDbComparison.Couchbase.Bug;

public static class GetById
{
    public record GetBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder GetBugByIdWithCouchbaseDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/{id:guid}", Handler)
            .WithName("GetBugById")
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugCouchbaseDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            var getOptions = new GetOptions();
            getOptions.CancellationToken(token);

            try
            {
                var getResult = await bugsDb.BugCollection.GetAsync(id.ToString(), getOptions);
                var getBug = getResult.ContentAs<GetBug>();
                return TypedResults.Ok(getBug);
            }
            catch (DocumentNotFoundException)
            {
                return TypedResults.NotFound();
            }
        };
}