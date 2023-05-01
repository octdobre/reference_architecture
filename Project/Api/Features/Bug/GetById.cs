using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Bug;

public static class GetById
{
    public class GetBug
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ReportTime { get; set; }
    }

    public static void SetupGetByIdBug(this RouteGroupBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapGet("/{id:guid}", Handler)
            .WithName("GetBugById")
            .WithOpenApi();
    }

    private static readonly Func<Guid, BugDb, CancellationToken, Task<IResult>> Handler = async (id, bugsDb, token) =>
    {
        return await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is { } bug
            ? TypedResults.Ok(
                new GetBug
                {
                    Id = bug.Id,
                    Title = bug.Title,
                    Description = bug.Description,
                    ReportTime = bug.ReportTime
                })
            : TypedResults.NotFound(new
            {
                NotFoundId = id
            });
    };
}