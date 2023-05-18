using System.Text;
using System.Text.Json;

namespace DocumentDbComparison.CouchDb.Bug;

public static class GetPaginated
{
    public record PagedBug(
        Guid _Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public record Page(
        IEnumerable<PagedBug> Bugs,
        int Displayed,
        int Total);

    public record FindResult(IEnumerable<PagedBug> Docs);

    public static RouteGroupBuilder GetPaginatedBugsWithCouchDb(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/", Handler)
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);

        return routeGroupBuilder;
    }

    private static readonly Func<int?, int?, bool?, BugCouchDbRepo, CancellationToken, Task<IResult>> Handler
        = async (pageNumber, pageSize, sortByTitle, bugsDb, token) =>
        {
            //default values
            pageNumber ??= 1;

            pageSize = pageSize is { }
                ? pageSize > 10 || pageSize < 0 ? 10
                : pageSize : 10;

            sortByTitle ??= true;
            var skip = (pageNumber - 1) * pageSize;

            var sortIdentifier = sortByTitle.Value ? @" ""asc"" " : @" ""desc"" ";
            var findQuery = @"
{
    ""selector"":{
        ""_id"": {
            ""$gt"": null
        }
    },
    ""fields"": [ ""_id"", ""Title"", ""Description"", ""ReportTime"" ],
    ""limit"" : " +  pageSize + @"," +
@"
    ""skip""  : " + skip + @"," +
@"
    ""sort"" : [{ ""Title"":" + sortIdentifier + @"}] " +
@"
}";
            var response = await bugsDb.Client.GetAsync($"{bugsDb.DatabaseName}/_design/bug/_view/count", token);
            var countView = JsonSerializer.Deserialize<CountView>(await response.Content.ReadAsStringAsync(token), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (countView is { } && countView.rows.First().Value > 0)
            {
                var content = new StringContent(findQuery, Encoding.UTF8, "application/json");
                response = await bugsDb.Client.PostAsync($"{bugsDb.DatabaseName}/_find", content, token);

                if (response.IsSuccessStatusCode)
                {
                    var findResult = JsonSerializer.Deserialize<FindResult>(await response.Content.ReadAsStringAsync(token), new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (findResult!.Docs.Any())
                    {
                        return TypedResults.Ok(
                            new Page(
                                findResult!.Docs.Select(b => new PagedBug(b._Id, b.Title, b.Description, b.ReportTime)),
                                findResult!.Docs.Count(),
                                (int)countView.rows.First().Value)
                        );
                    }

                    return TypedResults.NoContent();
                }
            }

            return TypedResults.BadRequest(await response.Content.ReadAsStringAsync());
        };
}

record CountView(Row[] rows);

record Row(string Key, int Value);

