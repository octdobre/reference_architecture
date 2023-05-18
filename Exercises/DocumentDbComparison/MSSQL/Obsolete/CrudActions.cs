namespace DocumentDbComparison.MSSQL.Obsolete;

/// <summary>
/// This class represents an example of functional programming
/// where the endpoints are functions and injection is done through parameter injection
/// and are build using functional composing.
/// </summary>
[Obsolete]
public static class CrudActions
{
    public record CreateBug(string Title, string Description, DateTime ReportTime);
    public record GetBug(Guid Id, string Title, string Description, DateTime ReportTime);
    public record UpdateBug(string? Title, string? Description);
    public record Page(IEnumerable<GetBug> Bugs, int Total);

    public static void SetupBugEndpoints<T>(
        this WebApplication app,
        Func<T, Guid, CreateBug, CancellationToken, Task<GetBug?>> creatorFunc,
        Func<T, Guid, CancellationToken, Task<GetBug?>> getByIdFunc,
        Func<T, int, int, bool, CancellationToken, Task<Page?>> getPagedFunc,
        Func<T, Guid, UpdateBug, CancellationToken, Task<GetBug?>> updateFunc,
        Func<T, Guid, CancellationToken, Task<Guid?>> deleteFunc)
    {
        var routeGroupBuilder = app.MapGroup($"/{typeof(T).Name.ToLower()}/bug");

        var toUpperDbName = typeof(T).Name.ToUpper();

        routeGroupBuilder.MapPost("/", HttpPostHandler(creatorFunc))
            .WithName($"{toUpperDbName}CreateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        routeGroupBuilder.MapGet("/{id:guid}", HttpGetHandler(getByIdFunc))
            .WithName($"{toUpperDbName}GetBug")
            .WithOpenApi()
            .Produces<GetBug>()
            .Produces(StatusCodes.Status404NotFound);

        routeGroupBuilder.MapGet("/", HttpGetPagedHandler(getPagedFunc))
            .WithName("GetBugsPaginated")
            .WithOpenApi()
            .Produces<Page>()
            .Produces(StatusCodes.Status404NotFound);

        routeGroupBuilder.MapPut("/{id:guid}", HttpPutHandler(updateFunc))
            .WithName($"{toUpperDbName}UpdateBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        routeGroupBuilder.MapDelete("/{id:guid}", HttpDeleteHandler(deleteFunc))
            .WithName($"{toUpperDbName}DeleteBug")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static Func<CreateBug, T, LinkGenerator, CancellationToken, Task<IResult>> HttpPostHandler<T>
        (Func<T, Guid, CreateBug, CancellationToken, Task<GetBug?>> creatorFunc)
    {
        return async (createResource, repository, linker, token) =>
        {
            var newId = Guid.NewGuid();

            return await creatorFunc(repository, newId, createResource, token) is { } resource
                ? TypedResults.Created($"{linker.GetPathByName("CreateBug")}/{newId}", resource)
                : TypedResults.BadRequest();
        };
    }

    private static Func<Guid, T, CancellationToken, Task<IResult>> HttpGetHandler<T>
        (Func<T, Guid, CancellationToken, Task<GetBug?>> getByIdFunc)
    {
        return async (id, repository, token) =>
        {
            return await getByIdFunc(repository, id, token) is { } resource
                ? TypedResults.Ok(resource)
                : TypedResults.NotFound();
        };
    }

    private static Func<int?, int?, bool?, T, CancellationToken, Task<IResult>> HttpGetPagedHandler<T>
        (Func<T, int, int, bool, CancellationToken, Task<Page?>> getPagedFunc)
    {
        return async (pageNumber, pageSize, sortByTitle, repository, token) =>
        {
            pageNumber ??= 1;
            pageSize = pageSize is { }
                ? pageSize is > 10 or < 0 ? 10
                : pageSize : 10;

            sortByTitle ??= true;
            var skip = (pageNumber - 1) * pageSize;

            return await getPagedFunc(repository, skip.Value, pageSize.Value, sortByTitle.Value, token) is { } page
                ? TypedResults.Ok(page)
                : TypedResults.NoContent();
        };
    }

    private static Func<Guid, UpdateBug, T, CancellationToken, Task<IResult>> HttpPutHandler<T>
        (Func<T, Guid, UpdateBug, CancellationToken, Task<GetBug?>> updateFunc)
    {
        return async (id, updateResource, repository, token) =>
        {
            return await updateFunc(repository, id, updateResource, token) is { } resource
                ? TypedResults.Ok(resource)
                : TypedResults.NoContent();
        };
    }

    private static Func<Guid, T, CancellationToken, Task<IResult>> HttpDeleteHandler<T>
        (Func<T, Guid, CancellationToken, Task<Guid?>> deleteFunc)
    {
        return async (id, repository, token) =>
        {
            return await deleteFunc(repository, id, token) is { } resourceIdentity
                ? TypedResults.Ok(resourceIdentity)
                : TypedResults.NotFound();
        };
    }
}