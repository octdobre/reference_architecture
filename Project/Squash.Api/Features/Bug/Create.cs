using MongoDB.Driver;
using Squash.Api.Infrastructure;

namespace Squash.Api.Features.Bug;

public static class Create
{
    private const string PathName = "CreateBug";

    public record CreateRequest(
        string Title,
        string Description,
        DateTime ReportTime);

    public static void SetupCreateBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName(PathName)
            .WithOpenApi()
            //validation filter
            .AddEndpointFilter(ValidationFilter)
            //logging filter
            .AddEndpointFilter(LoggingFilter)
            .Produces(StatusCodes.Status201Created);
    }

    private static readonly Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>> LoggingFilter
        = (context, next) =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Bug.Create).FullName);
            logger.LogInformation("Starting to create a bug!");
            var result = next(context);
            logger.LogInformation("Finished creating a bug!");
            return result;
        };

    private static readonly Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>> ValidationFilter
        = async (context, next) =>
        {
            var createRequest = context.GetArgument<CreateRequest>(0);

            if (createRequest.Title.Length > 10)
            {
                return TypedResults.BadRequest(new { Reason = "Title too long." });
            }

            if (createRequest.Description.Length > 144)
            {
                return TypedResults.BadRequest(new { Reason = "Description too long." });
            }

            return await next(context);
        };

    private static readonly Func<CreateRequest, BugDocumentDb, LinkGenerator, CancellationToken, Task<IResult>> Handler
        = async (createdBug, bugsDb, linker, token) =>
        {
            var newBug = new BugDocumentDb.BugDocument(
                Guid.NewGuid(),
                createdBug.Title,
                createdBug.Description,
            createdBug.ReportTime);

            await bugsDb.BugCollection.InsertOneAsync(newBug, new InsertOneOptions(), token);

            return TypedResults.Created($"{linker.GetPathByName(PathName)}/{newBug.Id}", newBug);
        };
}