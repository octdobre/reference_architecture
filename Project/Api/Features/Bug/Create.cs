using System.Net;
using Api.Infrastructure;

namespace Api.Features.Bug;

public static class Create
{
    private const string PathName = "CreateBug";

    public class CreateRequest
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ReportTime { get; set; }
    }

    public static void SetupCreateBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPost("/", Handler)
            .WithName(PathName)
            .WithOpenApi()
            //validation filter
            .AddEndpointFilter(async (context, next) =>
            {
                var createRequest = context.GetArgument<CreateRequest>(0);

                if (createRequest.Title.Length > 10)
                {
                    return await Task.FromResult(Results.BadRequest("Title too long."));
                }

                if(createRequest.Description.Length > 144)
                {
                    return await Task.FromResult(Results.BadRequest("Description too long."));
                }

                var result = next(context);
                return result;
            })
            //logging filter
            .AddEndpointFilter((context, next) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Api.Features.Bug.Create");
                logger.LogInformation("Starting to create a bug!");
                var result =  next(context);
                logger.LogInformation("Finished creating a bug!");
                return result;
            });
    }

    private static readonly Func<CreateRequest, BugDb, LinkGenerator, CancellationToken, Task<IResult>> Handler = async (createdBug, bugsDb, linker, token) =>
    {
        var newBug = new BugDb.BugDocument(
            Guid.NewGuid(),
            createdBug.Title,
            createdBug.Description,
            createdBug.ReportTime);

        await bugsDb.Bugs.AddAsync(newBug, token);

        await bugsDb.SaveChangesAsync(token);

        return TypedResults.Created($"{linker.GetPathByName(PathName)}/{newBug.Id}", newBug);
    };
}