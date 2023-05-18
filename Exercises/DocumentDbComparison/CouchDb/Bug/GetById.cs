using System.Text.Json;

namespace DocumentDbComparison.CouchDb.Bug;

public static class GetById
{
    public record GetBug(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder GetBugByIdWithCouchDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/{id:guid}", Handler)
            .WithName("GetBugById")
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugCouchDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            var response = await bugsDb.Client.GetAsync($"{bugsDb.DatabaseName}/{id}", token);

            if (response.IsSuccessStatusCode)
            {
                var returnedBug = JsonSerializer.Deserialize<GetBug>(await response.Content.ReadAsStringAsync(token));
                return TypedResults.Ok(returnedBug);
            }
            else
            {
                return TypedResults.NotFound();
            }
        };
}