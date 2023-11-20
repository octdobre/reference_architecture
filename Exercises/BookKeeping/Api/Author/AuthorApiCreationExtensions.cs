using BookKeeping.Repo;

namespace BookKeeping.Api.Author;

/// <summary>
/// Creation extensions cover:
/// - Adding multiple entities with AddRange()
/// </summary>
public static class AuthorApiCreationExtensions
{
    public static void AddAuthorApiCreationExtensions(this RouteGroupBuilder group)
    {
        group = group.MapGroup("/create");

        group.MapPost("/createMultiple", CreateMultipleHandler)
            .WithName("CreateMultipleAuthors")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);
    }

    private static readonly Func<ICollection<Domain.Author>, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateMultipleHandler
        = async (authors, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Authors.AddRange(authors);

            await bookKeepingContext.SaveChangesAsync(token);

            var idList = string.Join(',', authors.Select(a => a.Identity).Select(i => i.ToString()));

            return TypedResults.Created($"{linker.GetPathByName(AuthorApiQueryExtensions.PathName)}?ids={idList}");
        };
}

