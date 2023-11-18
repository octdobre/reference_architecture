using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.BookContent;

/// <summary>
/// One-to-One required relationship.
/// Shadow property writing.
/// Shadow property reading.
/// Querying with shadow properties.
/// </summary>
public static class BookContentApi
{
    private const string ResourceName = "BookContent";

    private const string CreatePathName = $"Create{ResourceName}";

    private const string BookIdentity = "BookId";
    public static void AddBookContentApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}")
            //groups endpoints like controllers
            .WithTags(ResourceName);

        group.MapPost("/createForBook/{bookId:guid}", CreateHandler)
            .WithName(CreatePathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        group.MapGet("/{id:guid}", GetByIdentity)
            .WithName($"Get{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateHandler)
            .WithName($"Update{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteHandler)
            .WithName($"Delete{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/byBook", GetByBookIdentity)
            .WithName($"Get{ResourceName}ByBook")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    /*
     *INSERT INTO [BookContent] ([Identity], [Content])
      VALUES (@p0, @p1, @p2);
     */
    private static readonly Func<Guid, Domain.BookContent, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (bookId, bookContent, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.BookContents.Add(bookContent);

            bookKeepingContext.Entry(bookContent).Property(BookIdentity).CurrentValue = bookId;

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{bookContent.Identity}", bookContent);
        };

    /*
     *SELECT TOP(2) [b].[Identity], [b].[Content]
      FROM [BookContent] AS [b]
      WHERE [b].[Identity] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token) =>
        {
            var bookContent = await bookKeepingContext.BookContents
                //.AsNoTracking() <- a small catch here, beware shadow properties are not loaded with AsNoTracking
                .SingleOrDefaultAsync(b => b.Identity == id, token);

            if (bookContent is { })
            {
                var bookIdentity = bookKeepingContext.Entry(bookContent).Property<Guid>(BookIdentity).CurrentValue;

                return TypedResults.Ok(
                    new
                    {
                        Identity = bookContent.Identity,
                        Content = bookContent.Content,
                        BookIdentity = bookIdentity
                    });
            }
            else
            {
                return TypedResults.NotFound();
            }
        };

    /*
     *SELECT TOP(2) [b].[Identity], [b].[Content]
      FROM [BookContent] AS [b]
      WHERE [b].[Identity] = @__id_0

      UPDATE [BookContent] SET [Content] = @p1
      OUTPUT 1
      WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.BookContent, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updateBookContent, bookKeepingContext, token) =>
        {
            if (await bookKeepingContext.BookContents.SingleOrDefaultAsync(b => b.Identity == id, token) is { } bookContent)
            {
                bookContent.Content = updateBookContent.Content;
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(bookContent);
            }

            return TypedResults.NotFound();
        };

    /*
      SELECT TOP(2) [b].[Identity], [b].[Content]
      FROM [BookContent] AS [b]
      WHERE [b].[Identity] = @__id_0

     *DELETE FROM [BookContent]
      OUTPUT 1
      WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingBookContent = await bookKeepingContext.BookContents.SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingBookContent is {})
            {
                bookKeepingContext.Remove(existingBookContent);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };

    /*
     * SELECT TOP(2) [b].[Identity], [b].[BookId], [b].[Content]
       FROM [BookContents] AS [b]
       WHERE [b].[BookId] = @__bookId_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByBookIdentity
        = async (bookId, bookKeepingContext, token) =>
        {
            var bookContent = await bookKeepingContext.BookContents
                .SingleOrDefaultAsync(b => EF.Property<Guid>(b, BookIdentity) == bookId, token);

            if (bookContent is { })
            {
                return TypedResults.Ok(
                    new
                    {
                        Identity = bookContent.Identity,
                        Content = bookContent.Content,
                        BookIdentity = bookKeepingContext.Entry(bookContent).Property(BookIdentity).CurrentValue
                    });
            }
            else
            {
                return TypedResults.NotFound();
            }
        };
}
