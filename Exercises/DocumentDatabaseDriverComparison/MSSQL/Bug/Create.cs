namespace DocumentDatabaseDriverComparison.MSSQL.Bug;

public static class Create
{
    public record CreateRequest(
        string Title,
        string Description,
        DateTime ReportTime);

    public static RouteGroupBuilder PostBugWithSql(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName("CreateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        return routeGroupBuilder;
    }

    private static readonly Func<CreateRequest, BugSqlDb, LinkGenerator, CancellationToken, Task<IResult>> Handler
        = async (createdBug, bugsDb, linker, token) =>
        {
            var newBug = new BugSqlDb.BugRecord(
                Guid.NewGuid(),
                createdBug.Title,
                createdBug.Description,
                createdBug.ReportTime);

            bugsDb.Bugs.Add(newBug);

            await bugsDb.SaveChangesAsync(token);

            return CreateGetResponse(linker, newBug.Id, newBug);
        };

    private static IResult CreateGetResponse<T>(LinkGenerator linker, Guid id, T? resource)
    {
        return resource is { }
            ? TypedResults.Created($"{linker.GetPathByName("CreateBug")}/{id}", resource)
            : TypedResults.BadRequest();
    }
}