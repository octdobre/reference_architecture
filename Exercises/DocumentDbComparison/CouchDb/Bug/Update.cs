using System.Text;
using System.Text.Json;

namespace DocumentDbComparison.CouchDb.Bug;

public static class Update
{
    public record UpdateBug(
        string? Title,
        string? Description);

    record UpdateBugWithRevision(
        string _rev,
        string? Title,
        string? Description);

    public static RouteGroupBuilder UpdateBugWithCouchDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPut("/{id:guid}", Handler)
            .WithName("UpdateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, UpdateBug, BugCouchDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, updateBug, bugsDb, token) =>
    {

        var response = await bugsDb.Client.GetAsync($"{bugsDb.DatabaseName}/{id}", token);

        if (response.IsSuccessStatusCode)
        {
            var returnedBug = JsonSerializer.Deserialize<UpdateBugWithRevision>(await response.Content.ReadAsStringAsync(token));

            var updatedBug = returnedBug! with { Title = updateBug.Title, Description = updateBug.Description };

            var content = new StringContent(JsonSerializer.Serialize(updatedBug), Encoding.UTF8, "application/json");

            response = await bugsDb.Client.PutAsync($"{bugsDb.DatabaseName}/{id}", content, token);

            if (response.IsSuccessStatusCode)
            {
                return TypedResults.Ok();
            }

            return TypedResults.BadRequest(response.Content.ReadAsStringAsync().Result);
        }
        else
        {
            return TypedResults.NotFound();
        }
    };
}