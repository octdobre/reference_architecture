using System.Text.Json;

namespace NoSqlComparison.CouchDb.Bug;

public static class Delete
{
    class DeleteBug
    {
        public string _rev { get; set; }
    }
    public static RouteGroupBuilder DeleteBugWithCouchDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapDelete("/{id:guid}", Handler)
            .WithName("DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<Guid, BugCouchDbRepo, CancellationToken, Task<IResult>> Handler
        = async (id, bugsDb, token) =>
        {
            var response = await bugsDb.Client.GetAsync($"{bugsDb.DatabaseName}/{id}", token);

            if (response.IsSuccessStatusCode)
            {
                var bugToDelete = JsonSerializer.Deserialize<DeleteBug>(await response.Content.ReadAsStringAsync(token));

                //?rev adds revision of document
                response = await bugsDb.Client.DeleteAsync($"{bugsDb.DatabaseName}/{id}?rev={bugToDelete!._rev}", token);

                if (response.IsSuccessStatusCode)
                {
                    return TypedResults.Ok(id);
                }
                else
                {
                    return TypedResults.NotFound();
                }
            }

            return TypedResults.BadRequest(await response.Content.ReadAsStringAsync(token));
        };
}