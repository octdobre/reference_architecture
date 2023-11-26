using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Publisher;

/// <summary>
/// Example of JOIN statement.
/// </summary>
public static class PublisherApiQueryExtensions
{
    public static void AddPublisherApiQueryExtensions(this RouteGroupBuilder group)
    {
        group = group.MapGroup("/{publisherId:guid}/query");

        group.MapPost("/joinedWithEditor", JoinedWithEditorQueryHandler)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);
    }

    /*
      SELECT TOP(2) [p].[Identity] AS [PublisherIdentity], [p].[FirstName] AS [PublisherName]
      FROM [Publishers] AS [p]
      INNER JOIN [Editors] AS [e] ON [p].[Identity] = [e].[PublisherId]
      WHERE [p].[Identity] = @__publisherId_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> JoinedWithEditorQueryHandler
        = async (publisherId, bookKeepingContext, token) =>
        {
            var publisherView = await (from publisher in bookKeepingContext.Publishers
                join editor in bookKeepingContext.Editors on publisher.Identity equals editor.PublisherId
                where publisher.Identity == publisherId
                select new
                {
                    PublisherIdentity = publisher.Identity,
                    PublisherName = publisher.Name
                }).SingleOrDefaultAsync(token);

            return publisherView is not null
                ? TypedResults.Ok(publisherView)
                : TypedResults.NotFound();
        };
}
