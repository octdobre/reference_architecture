using System.Text;
using System.Text.Json;
using DocumentDbComparison.MongoDb;

namespace DocumentDbComparison.CouchDb.Bug;

public static class Create
{
    private const string PathName = "CreateBug";

    public record CreateRequest(
        string Title,
        string Description,
        DateTime ReportTime);

    class CreateResponse
    {
        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }
    }

    public static RouteGroupBuilder PostBugWithCouchDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName(PathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        return routeGroupBuilder;
    }

    private static readonly Func<CreateRequest, BugCouchDbRepo, LinkGenerator, CancellationToken, Task<IResult>> Handler
        = async (createBug, bugsDb, linker, token) =>
        {
            var newBug = new BugMongoDbRepo.BugDocument(
                Guid.NewGuid(),
                createBug.Title,
                createBug.Description,
                createBug.ReportTime);

            var content = new StringContent(JsonSerializer.Serialize(newBug), Encoding.UTF8, "application/json");

            var response = await bugsDb.Client.PutAsync($"{bugsDb.DatabaseName}/{newBug.Id}", content, token);

            if (response.IsSuccessStatusCode)
            {
                var responseMessage = JsonSerializer.Deserialize<CreateResponse>(await response.Content.ReadAsStringAsync(token), new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
                return TypedResults.Created($"{linker.GetPathByName(PathName)}/{newBug.Id}/{responseMessage!.Rev}", newBug);
            }

            return TypedResults.BadRequest(await response.Content.ReadAsStringAsync(token));
        };
}